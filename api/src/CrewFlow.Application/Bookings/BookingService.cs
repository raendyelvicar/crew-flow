using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Bookings;

public class BookingService
{
    private readonly IAppDbContext _db;

    public BookingService(IAppDbContext db)
    {
        _db = db;
    }

    // Books a seat, or waitlists if the occurrence is at capacity. Runs inside a
    // serializable transaction and retries once on a serialization failure, so two
    // members racing for the last seat can't both succeed.
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await CreateBookingInternalAsync(request, ct);
            }
            catch (DbUpdateException) when (attempt < maxAttempts)
            {
                await Task.Delay(50 * attempt, ct);
            }
        }

        throw new ConflictException("Could not complete booking due to a concurrent update - please try again.");
    }

    private async Task<BookingResponse> CreateBookingInternalAsync(CreateBookingRequest request, CancellationToken ct)
    {
        await using var transaction = await _db.BeginTransactionAsync(ct);

        var occurrence = await _db.ClassOccurrences
            .Include(o => o.Bookings)
            .FirstOrDefaultAsync(o => o.Id == request.ClassOccurrenceId, ct)
            ?? throw new NotFoundException(nameof(ClassOccurrence), request.ClassOccurrenceId);

        if (occurrence.Status != OccurrenceStatus.Scheduled)
        {
            throw new ConflictException("This class occurrence is not open for booking.");
        }

        var alreadyActive = occurrence.Bookings.Any(b =>
            b.MemberId == request.MemberId && b.Status is BookingStatus.Booked or BookingStatus.Waitlisted);
        if (alreadyActive)
        {
            throw new ConflictException("This member already has an active booking for this class.");
        }

        var (paymentMethod, creditPackPurchaseId) = await ResolvePaymentMethodAsync(request.MemberId, ct);

        var bookedCount = occurrence.Bookings.Count(b => b.Status == BookingStatus.Booked);
        var status = bookedCount < occurrence.Capacity ? BookingStatus.Booked : BookingStatus.Waitlisted;

        int? waitlistPosition = null;
        if (status == BookingStatus.Waitlisted)
        {
            waitlistPosition = occurrence.Bookings.Count(b => b.Status == BookingStatus.Waitlisted) + 1;
        }

        if (paymentMethod == BookingPaymentMethod.Credit)
        {
            await SpendCreditAsync(creditPackPurchaseId!.Value, ct);
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ClassOccurrenceId = occurrence.Id,
            MemberId = request.MemberId,
            Status = status,
            PaymentMethod = paymentMethod,
            CreditPackPurchaseId = creditPackPurchaseId,
            WaitlistPosition = waitlistPosition,
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return MapBooking(booking);
    }

    public async Task<BookingResponse> CancelBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
        {
            throw new ConflictException("This booking cannot be cancelled.");
        }

        var wasBooked = booking.Status == BookingStatus.Booked;
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;

        if (booking.PaymentMethod == BookingPaymentMethod.Credit && booking.CreditPackPurchaseId is not null)
        {
            var purchase = await _db.CreditPackPurchases.FirstOrDefaultAsync(p => p.Id == booking.CreditPackPurchaseId, ct);
            if (purchase is not null)
            {
                purchase.CreditsRemaining += 1;
                if (purchase.Status == CreditPackPurchaseStatus.Depleted)
                {
                    purchase.Status = CreditPackPurchaseStatus.Active;
                }
            }
        }

        if (wasBooked)
        {
            var nextWaitlisted = await _db.Bookings
                .Where(b => b.ClassOccurrenceId == booking.ClassOccurrenceId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Booked;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.PromotedFromWaitlistAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        return MapBooking(booking);
    }

    public async Task<BookingResponse> MarkAttendanceAsync(Guid bookingId, MarkAttendanceRequest request, CancellationToken ct = default)
    {
        if (request.Status is not (BookingStatus.Attended or BookingStatus.NoShow))
        {
            throw new ConflictException("Status must be Attended or NoShow.");
        }

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        booking.Status = request.Status;
        await _db.SaveChangesAsync(ct);
        return MapBooking(booking);
    }

    // Used to authorize a Coach's attendance check-ins to only their own classes.
    public async Task<Guid> GetOccurrenceInstructorIdForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.AsNoTracking()
            .Include(b => b.ClassOccurrence)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        return booking.ClassOccurrence?.InstructorUserId
            ?? throw new NotFoundException(nameof(ClassOccurrence), booking.ClassOccurrenceId);
    }

    public async Task<IReadOnlyList<MyBookingResponse>> GetMemberBookingsAsync(Guid memberId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings.AsNoTracking()
            .Include(b => b.ClassOccurrence).ThenInclude(o => o!.Activity)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookedAtUtc)
            .ToListAsync(ct);

        return bookings.Select(b => new MyBookingResponse(
            b.Id,
            b.ClassOccurrenceId,
            b.ClassOccurrence?.Activity?.Name ?? string.Empty,
            b.ClassOccurrence?.StartAtUtc ?? default,
            b.Status,
            b.WaitlistPosition)).ToList();
    }

    public async Task<IReadOnlyList<RosterEntryResponse>> GetRosterAsync(Guid occurrenceId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassOccurrenceId == occurrenceId && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.Status).ThenBy(b => b.WaitlistPosition)
            .ToListAsync(ct);

        return bookings.Select(b => new RosterEntryResponse(
            b.Id, b.MemberId, $"{b.Member?.FirstName} {b.Member?.LastName}".Trim(), b.Status, b.WaitlistPosition, b.BookedAtUtc))
            .ToList();
    }

    public async Task<ClassReviewResponse> CreateReviewAsync(CreateClassReviewRequest request, CancellationToken ct = default)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new ConflictException("Rating must be between 1 and 5.");
        }

        var attended = await _db.Bookings.AnyAsync(b =>
            b.ClassOccurrenceId == request.ClassOccurrenceId
            && b.MemberId == request.MemberId
            && b.Status == BookingStatus.Attended, ct);

        if (!attended)
        {
            throw new ConflictException("Only members who attended this class can leave a review.");
        }

        var alreadyReviewed = await _db.ClassReviews.AnyAsync(r =>
            r.ClassOccurrenceId == request.ClassOccurrenceId && r.MemberId == request.MemberId, ct);
        if (alreadyReviewed)
        {
            throw new ConflictException("You have already reviewed this class.");
        }

        var occurrence = await _db.ClassOccurrences.FirstOrDefaultAsync(o => o.Id == request.ClassOccurrenceId, ct)
            ?? throw new NotFoundException(nameof(ClassOccurrence), request.ClassOccurrenceId);

        var review = new ClassReview
        {
            Id = Guid.NewGuid(),
            ClassOccurrenceId = request.ClassOccurrenceId,
            MemberId = request.MemberId,
            InstructorUserId = occurrence.InstructorUserId,
            Rating = request.Rating,
            Comment = request.Comment,
        };

        _db.ClassReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        return MapReview(review);
    }

    public async Task<IReadOnlyList<ClassReviewResponse>> GetReviewsForInstructorAsync(Guid instructorUserId, CancellationToken ct = default)
    {
        var reviews = await _db.ClassReviews.AsNoTracking()
            .Where(r => r.InstructorUserId == instructorUserId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(ct);

        return reviews.Select(MapReview).ToList();
    }

    private async Task<(BookingPaymentMethod Method, Guid? CreditPackPurchaseId)> ResolvePaymentMethodAsync(Guid memberId, CancellationToken ct)
    {
        var hasActiveSubscription = await _db.Subscriptions.AnyAsync(s =>
            s.MemberId == memberId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing), ct);

        if (hasActiveSubscription)
        {
            return (BookingPaymentMethod.Subscription, null);
        }

        var candidatePurchases = await _db.CreditPackPurchases
            .Where(p => p.MemberId == memberId && p.Status == CreditPackPurchaseStatus.Active && p.CreditsRemaining > 0)
            .OrderBy(p => p.ExpiresAtUtc ?? DateTime.MaxValue)
            .ToListAsync(ct);

        var usablePurchase = candidatePurchases.FirstOrDefault(p => p.HasUsableCredits);
        if (usablePurchase is not null)
        {
            return (BookingPaymentMethod.Credit, usablePurchase.Id);
        }

        throw new ConflictException("An active subscription or class credit is required to book this class.");
    }

    private async Task SpendCreditAsync(Guid creditPackPurchaseId, CancellationToken ct)
    {
        var purchase = await _db.CreditPackPurchases.FirstOrDefaultAsync(p => p.Id == creditPackPurchaseId, ct)
            ?? throw new NotFoundException(nameof(CreditPackPurchase), creditPackPurchaseId);

        purchase.CreditsRemaining -= 1;
        if (purchase.CreditsRemaining <= 0)
        {
            purchase.Status = CreditPackPurchaseStatus.Depleted;
        }
    }

    private static BookingResponse MapBooking(Booking b) => new(
        b.Id, b.ClassOccurrenceId, b.MemberId, b.Status, b.PaymentMethod, b.BookedAtUtc, b.WaitlistPosition, b.CancelledAtUtc);

    private static ClassReviewResponse MapReview(ClassReview r) => new(
        r.Id, r.ClassOccurrenceId, r.MemberId, r.InstructorUserId, r.Rating, r.Comment, r.CreatedAtUtc);
}
