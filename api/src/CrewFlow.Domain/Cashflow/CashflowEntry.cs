using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;

namespace CrewFlow.Domain.Cashflow;

public class CashflowEntry
{
    public Guid Id { get; set; }

    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }

    // Positive = income, negative = refund/reversal.
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public CashflowSource Source { get; set; }
    public CashflowCategory Category { get; set; }
    public string? Description { get; set; }

    // Stripe charge/invoice id - unique when set, guards webhook-retry idempotency.
    public string? ReferenceStripeObjectId { get; set; }

    public Guid? RecordedByUserId { get; set; }
    public ApplicationUser? RecordedByUser { get; set; }

    public DateTime OccurredAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ReconciliationStatus ReconciliationStatus { get; set; } = ReconciliationStatus.Unreconciled;
}
