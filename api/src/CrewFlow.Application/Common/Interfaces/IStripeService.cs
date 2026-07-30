namespace CrewFlow.Application.Common.Interfaces;

public record StripeCheckoutSessionResult(string SessionId, string CheckoutUrl);

// Flattened, provider-agnostic view of a Stripe webhook event - keeps Stripe.net types
// out of the Application layer. Populated from whichever fields are relevant to EventType.
public record StripeWebhookEvent(
    string EventId,
    string EventType,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    string? StripeInvoiceId,
    string? StripePaymentIntentId,
    string? SubscriptionStatus,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc,
    bool CancelAtPeriodEnd,
    long? AmountPaid,
    string? Currency,
    IReadOnlyDictionary<string, string> Metadata);

public interface IStripeService
{
    Task<string> GetOrCreateCustomerAsync(Guid memberId, string email, string name, CancellationToken ct = default);

    Task<StripeCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        string stripeCustomerId,
        string stripePriceId,
        string successUrl,
        string cancelUrl,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task<StripeCheckoutSessionResult> CreateCreditPackCheckoutSessionAsync(
        string stripeCustomerId,
        string stripePriceId,
        string successUrl,
        string cancelUrl,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task CancelSubscriptionAsync(string stripeSubscriptionId, bool atPeriodEnd, CancellationToken ct = default);

    Task<(string ProductId, string PriceId)> UpsertPlanPriceAsync(
        string name,
        string? description,
        int priceAmount,
        string currency,
        string interval,
        string? existingProductId,
        CancellationToken ct = default);

    Task<(string ProductId, string PriceId)> UpsertOneTimePriceAsync(
        string name,
        string? description,
        int priceAmount,
        string currency,
        string? existingProductId,
        CancellationToken ct = default);

    StripeWebhookEvent ParseWebhookEvent(string payload, string signatureHeader);
}
