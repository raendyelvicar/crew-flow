using CrewFlow.Domain.Bookings;

namespace CrewFlow.Application.Bookings;

public record BookingResponse(
    Guid Id,
    Guid ClassOccurrenceId,
    Guid MemberId,
    BookingStatus Status,
    BookingPaymentMethod PaymentMethod,
    DateTime BookedAtUtc,
    int? WaitlistPosition,
    DateTime? CancelledAtUtc);

public record MyBookingResponse(
    Guid Id,
    Guid ClassOccurrenceId,
    string ActivityName,
    DateTime StartAtUtc,
    BookingStatus Status,
    int? WaitlistPosition);

public record RosterEntryResponse(
    Guid BookingId, Guid MemberId, string MemberName, BookingStatus Status, int? WaitlistPosition, DateTime BookedAtUtc);

public record CreateBookingRequest(Guid ClassOccurrenceId, Guid MemberId);

public record MarkAttendanceRequest(BookingStatus Status);

public record ClassReviewResponse(
    Guid Id, Guid ClassOccurrenceId, Guid MemberId, Guid InstructorUserId, int Rating, string? Comment, DateTime CreatedAtUtc);

public record CreateClassReviewRequest(Guid ClassOccurrenceId, Guid MemberId, int Rating, string? Comment);
