using CrewFlow.Domain.Billing;

namespace CrewFlow.Application.Billing;

public record MembershipPlanResponse(
    Guid Id, string Name, string? Description, BillingInterval BillingInterval, int PriceAmount, string Currency, bool IsActive, int SortOrder);

public record UpsertMembershipPlanRequest(
    string Name, string? Description, BillingInterval BillingInterval, int PriceAmount, string Currency, bool IsActive, int SortOrder);

public record SubscriptionResponse(
    Guid Id, Guid MemberId, Guid MembershipPlanId, string PlanName, SubscriptionStatus Status,
    DateTime? CurrentPeriodStartUtc, DateTime? CurrentPeriodEndUtc, bool CancelAtPeriodEnd);

public record CreateSubscriptionCheckoutRequest(Guid MemberId, Guid MembershipPlanId, string SuccessUrl, string CancelUrl);

public record CheckoutSessionResponse(string CheckoutUrl);

public record CreditPackResponse(
    Guid Id, string Name, string? Description, int CreditCount, int PriceAmount, string Currency, int? ExpiryDays, bool IsActive);

public record UpsertCreditPackRequest(
    string Name, string? Description, int CreditCount, int PriceAmount, string Currency, int? ExpiryDays, bool IsActive);

public record CreateCreditPackCheckoutRequest(Guid MemberId, Guid CreditPackId, string SuccessUrl, string CancelUrl);

public record CreditPackPurchaseResponse(
    Guid Id, Guid MemberId, Guid CreditPackId, string CreditPackName, int CreditsRemaining,
    DateTime PurchasedAtUtc, DateTime? ExpiresAtUtc, CreditPackPurchaseStatus Status);
