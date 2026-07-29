using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;

namespace CrewFlow.Domain.Bookings;

public class Booking
{
    public Guid Id { get; set; }

    public Guid ClassOccurrenceId { get; set; }
    public ClassOccurrence? ClassOccurrence { get; set; }

    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Booked;
    public BookingPaymentMethod PaymentMethod { get; set; }

    // Set only when PaymentMethod == Credit; one credit is spent/refunded alongside this booking.
    public Guid? CreditPackPurchaseId { get; set; }
    public CreditPackPurchase? CreditPackPurchase { get; set; }

    public DateTime BookedAtUtc { get; set; } = DateTime.UtcNow;
    public int? WaitlistPosition { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? PromotedFromWaitlistAtUtc { get; set; }
}
