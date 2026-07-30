namespace CrewFlow.Domain.Billing;

public class CreditPack
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditCount { get; set; }
    public int PriceAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }

    // Null means credits never expire.
    public int? ExpiryDays { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CreditPackPurchase> Purchases { get; set; } = new List<CreditPackPurchase>();
}
