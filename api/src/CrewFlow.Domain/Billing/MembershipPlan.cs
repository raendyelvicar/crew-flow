namespace CrewFlow.Domain.Billing;

public class MembershipPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BillingInterval BillingInterval { get; set; }
    public int PriceCents { get; set; }
    public string Currency { get; set; } = "usd";
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
