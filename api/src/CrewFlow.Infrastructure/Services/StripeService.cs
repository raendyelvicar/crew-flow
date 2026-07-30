using System.Text.Json;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace CrewFlow.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly StripeOptions _options;

    // Stripe's actual zero-decimal currency list - these are billed in whole units with no
    // "cents" multiplier. Everything else (including IDR, despite having no everyday
    // subunit) must be multiplied by 100 to become Stripe's unit_amount.
    private static readonly HashSet<string> StripeZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf",
    };

    public StripeService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    // `priceAmount` throughout this app is always the literal amount in the currency's major
    // unit (e.g. 400000 means Rp400,000) - this converts it to Stripe's unit_amount convention
    // right at the API boundary, so the rest of the app never has to think in "cents".
    private static long ToStripeUnitAmount(int priceAmount, string currency)
        => StripeZeroDecimalCurrencies.Contains(currency) ? priceAmount : priceAmount * 100L;

    public async Task<string> GetOrCreateCustomerAsync(Guid memberId, string email, string name, CancellationToken ct = default)
    {
        var customerService = new CustomerService();

        var searchResult = await customerService.SearchAsync(
            new CustomerSearchOptions { Query = $"metadata['memberId']:'{memberId}'" },
            cancellationToken: ct);

        if (searchResult.Data.Count > 0)
        {
            return searchResult.Data[0].Id;
        }

        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = new Dictionary<string, string> { ["memberId"] = memberId.ToString() },
        }, cancellationToken: ct);

        return customer.Id;
    }

    public async Task<StripeCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        string stripeCustomerId,
        string stripePriceId,
        string successUrl,
        string cancelUrl,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Mode = "subscription",
            Customer = stripeCustomerId,
            LineItems = [new SessionLineItemOptions { Price = stripePriceId, Quantity = 1 }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>(metadata),
            SubscriptionData = new SessionSubscriptionDataOptions { Metadata = new Dictionary<string, string>(metadata) },
        }, cancellationToken: ct);

        return new StripeCheckoutSessionResult(session.Id, session.Url);
    }

    public async Task<StripeCheckoutSessionResult> CreateCreditPackCheckoutSessionAsync(
        string stripeCustomerId,
        string stripePriceId,
        string successUrl,
        string cancelUrl,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Mode = "payment",
            Customer = stripeCustomerId,
            LineItems = [new SessionLineItemOptions { Price = stripePriceId, Quantity = 1 }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>(metadata),
            PaymentIntentData = new SessionPaymentIntentDataOptions { Metadata = new Dictionary<string, string>(metadata) },
        }, cancellationToken: ct);

        return new StripeCheckoutSessionResult(session.Id, session.Url);
    }

    public async Task CancelSubscriptionAsync(string stripeSubscriptionId, bool atPeriodEnd, CancellationToken ct = default)
    {
        var subscriptionService = new SubscriptionService();

        if (atPeriodEnd)
        {
            await subscriptionService.UpdateAsync(
                stripeSubscriptionId,
                new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
                cancellationToken: ct);
        }
        else
        {
            await subscriptionService.CancelAsync(stripeSubscriptionId, cancellationToken: ct);
        }
    }

    public async Task<(string ProductId, string PriceId)> UpsertPlanPriceAsync(
        string name, string? description, int priceAmount, string currency, string interval, string? existingProductId, CancellationToken ct = default)
    {
        var productId = await UpsertProductAsync(name, description, existingProductId, ct);

        // Stripe prices are immutable - changing the amount means creating a new price under the same product.
        var priceService = new PriceService();
        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = productId,
            UnitAmount = ToStripeUnitAmount(priceAmount, currency),
            Currency = currency,
            Recurring = new PriceRecurringOptions { Interval = interval },
        }, cancellationToken: ct);

        return (productId, price.Id);
    }

    public async Task<(string ProductId, string PriceId)> UpsertOneTimePriceAsync(
        string name, string? description, int priceAmount, string currency, string? existingProductId, CancellationToken ct = default)
    {
        var productId = await UpsertProductAsync(name, description, existingProductId, ct);

        var priceService = new PriceService();
        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = productId,
            UnitAmount = ToStripeUnitAmount(priceAmount, currency),
            Currency = currency,
        }, cancellationToken: ct);

        return (productId, price.Id);
    }

    private static async Task<string> UpsertProductAsync(string name, string? description, string? existingProductId, CancellationToken ct)
    {
        var productService = new ProductService();

        if (existingProductId is not null)
        {
            var updated = await productService.UpdateAsync(existingProductId, new ProductUpdateOptions
            {
                Name = name,
                Description = description,
            }, cancellationToken: ct);
            return updated.Id;
        }

        var created = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = name,
            Description = description,
        }, cancellationToken: ct);
        return created.Id;
    }

    // Parses the raw webhook JSON directly rather than casting Stripe.net's typed Event.Data.Object -
    // that typed object model shifts across SDK/Stripe API versions, while the raw field names used
    // here (customer, subscription, status, current_period_start/end, amount_paid, metadata, ...) are
    // stable across the event types this scaffold handles.
    public StripeWebhookEvent ParseWebhookEvent(string payload, string signatureHeader)
    {
        var verifiedEvent = EventUtility.ConstructEvent(payload, signatureHeader, _options.WebhookSecret);

        using var doc = JsonDocument.Parse(payload);
        var dataObject = doc.RootElement.GetProperty("data").GetProperty("object");

        string? customerId = GetString(dataObject, "customer");
        string? paymentIntentId = GetString(dataObject, "payment_intent");
        string? status = GetString(dataObject, "status");
        var cancelAtPeriodEnd = dataObject.TryGetProperty("cancel_at_period_end", out var capeEl)
            && capeEl.ValueKind == JsonValueKind.True;
        long? amountPaid = GetLong(dataObject, "amount_paid");
        string? currency = GetString(dataObject, "currency");

        string? subscriptionId = null;
        string? invoiceId = null;
        DateTime? periodStart = null;
        DateTime? periodEnd = null;

        if (verifiedEvent.Type.StartsWith("customer.subscription.", StringComparison.Ordinal))
        {
            subscriptionId = GetString(dataObject, "id");
            periodStart = GetUnixSeconds(dataObject, "current_period_start");
            periodEnd = GetUnixSeconds(dataObject, "current_period_end");
        }
        else if (verifiedEvent.Type.StartsWith("invoice.", StringComparison.Ordinal))
        {
            invoiceId = GetString(dataObject, "id");
            subscriptionId = GetString(dataObject, "subscription");
        }
        else if (verifiedEvent.Type == "checkout.session.completed")
        {
            subscriptionId = GetString(dataObject, "subscription");
        }

        var metadata = new Dictionary<string, string>();
        if (dataObject.TryGetProperty("metadata", out var metadataEl) && metadataEl.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in metadataEl.EnumerateObject())
            {
                metadata[prop.Name] = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() ?? string.Empty : string.Empty;
            }
        }

        return new StripeWebhookEvent(
            verifiedEvent.Id,
            verifiedEvent.Type,
            customerId,
            subscriptionId,
            invoiceId,
            paymentIntentId,
            status,
            periodStart,
            periodEnd,
            cancelAtPeriodEnd,
            amountPaid,
            currency,
            metadata);
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static long? GetLong(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt64() : null;

    private static DateTime? GetUnixSeconds(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? DateTimeOffset.FromUnixTimeSeconds(value.GetInt64()).UtcDateTime
            : null;
}
