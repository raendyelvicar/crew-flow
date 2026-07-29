using CrewFlow.Domain.Members;

namespace CrewFlow.Domain.Billing;

public class CreditPackPurchase
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public Guid CreditPackId { get; set; }
    public CreditPack? CreditPack { get; set; }

    public int CreditsRemaining { get; set; }
    public DateTime PurchasedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public CreditPackPurchaseStatus Status { get; set; } = CreditPackPurchaseStatus.Active;

    public bool HasUsableCredits =>
        Status == CreditPackPurchaseStatus.Active
        && CreditsRemaining > 0
        && (ExpiresAtUtc is null || ExpiresAtUtc > DateTime.UtcNow);
}
