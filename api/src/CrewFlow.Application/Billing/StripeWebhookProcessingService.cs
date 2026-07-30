using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Cashflow;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Billing;

// Maps flattened Stripe webhook events onto local Subscription/CashflowEntry/CreditPackPurchase
// state. Idempotency is guarded primarily by ProcessedStripeEvent (checked first, short-circuits
// retried deliveries) and secondarily by the unique index on CashflowEntry.ReferenceStripeObjectId.
public class StripeWebhookProcessingService
{
    private readonly IAppDbContext _db;

    public StripeWebhookProcessingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task ProcessAsync(StripeWebhookEvent evt, CancellationToken ct = default)
    {
        var alreadyProcessed = await _db.ProcessedStripeEvents.AnyAsync(e => e.StripeEventId == evt.EventId, ct);
        if (alreadyProcessed)
        {
            return;
        }

        switch (evt.EventType)
        {
            case "checkout.session.completed":
                await HandleCheckoutSessionCompletedAsync(evt, ct);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpsertAsync(evt, ct);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(evt, ct);
                break;
            case "invoice.paid":
            case "invoice.payment_succeeded":
                await HandleInvoicePaidAsync(evt, ct);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(evt, ct);
                break;
        }

        _db.ProcessedStripeEvents.Add(new ProcessedStripeEvent
        {
            Id = Guid.NewGuid(),
            StripeEventId = evt.EventId,
            EventType = evt.EventType,
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task HandleCheckoutSessionCompletedAsync(StripeWebhookEvent evt, CancellationToken ct)
    {
        if (evt.Metadata.TryGetValue("creditPackId", out var creditPackIdRaw)
            && evt.Metadata.TryGetValue("memberId", out var memberIdRaw)
            && Guid.TryParse(creditPackIdRaw, out var creditPackId)
            && Guid.TryParse(memberIdRaw, out var memberId))
        {
            var pack = await _db.CreditPacks.FirstOrDefaultAsync(p => p.Id == creditPackId, ct);
            if (pack is not null)
            {
                _db.CreditPackPurchases.Add(new CreditPackPurchase
                {
                    Id = Guid.NewGuid(),
                    MemberId = memberId,
                    CreditPackId = pack.Id,
                    CreditsRemaining = pack.CreditCount,
                    ExpiresAtUtc = pack.ExpiryDays is null ? null : DateTime.UtcNow.AddDays(pack.ExpiryDays.Value),
                    StripePaymentIntentId = evt.StripePaymentIntentId,
                    Status = CreditPackPurchaseStatus.Active,
                });
                await _db.SaveChangesAsync(ct);
            }
        }
        // Subscription checkout completion is handled by customer.subscription.created/updated,
        // which carries the definitive Stripe subscription id/status.
    }

    private async Task HandleSubscriptionUpsertAsync(StripeWebhookEvent evt, CancellationToken ct)
    {
        if (evt.StripeSubscriptionId is null) return;

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == evt.StripeSubscriptionId, ct);

        if (subscription is null && evt.StripeCustomerId is not null)
        {
            // First time we see this subscription id - attach it to the Incomplete placeholder
            // row created when checkout was started for this customer.
            subscription = await _db.Subscriptions
                .Where(s => s.StripeCustomerId == evt.StripeCustomerId && s.StripeSubscriptionId == null)
                .OrderByDescending(s => s.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);

            if (subscription is not null)
            {
                subscription.StripeSubscriptionId = evt.StripeSubscriptionId;
            }
        }

        if (subscription is null) return;

        // A changed period start means a new billing cycle rolled over (or this is the first
        // time the subscription becomes active) - the plan's per-period credit allotment
        // refreshes and any unused credits from the prior period are forfeited.
        var isNewPeriod = subscription.CurrentPeriodStartUtc != evt.CurrentPeriodStartUtc;

        subscription.Status = MapStripeStatus(evt.SubscriptionStatus);
        subscription.CurrentPeriodStartUtc = evt.CurrentPeriodStartUtc;
        subscription.CurrentPeriodEndUtc = evt.CurrentPeriodEndUtc;
        subscription.CancelAtPeriodEnd = evt.CancelAtPeriodEnd;

        if (isNewPeriod)
        {
            var plan = await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == subscription.MembershipPlanId, ct);
            if (plan is not null)
            {
                subscription.CreditsRemainingThisPeriod = plan.CreditsPerPeriod;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task HandleSubscriptionDeletedAsync(StripeWebhookEvent evt, CancellationToken ct)
    {
        if (evt.StripeSubscriptionId is null) return;

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == evt.StripeSubscriptionId, ct);
        if (subscription is null) return;

        subscription.Status = SubscriptionStatus.Canceled;
        subscription.CanceledAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task HandleInvoicePaidAsync(StripeWebhookEvent evt, CancellationToken ct)
    {
        if (evt.StripeInvoiceId is null || evt.AmountPaid is null) return;

        var alreadyRecorded = await _db.CashflowEntries.AnyAsync(e => e.ReferenceStripeObjectId == evt.StripeInvoiceId, ct);
        if (alreadyRecorded) return;

        Guid? memberId = null;
        if (evt.StripeSubscriptionId is not null)
        {
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == evt.StripeSubscriptionId, ct);
            memberId = subscription?.MemberId;
        }
        else if (evt.StripeCustomerId is not null)
        {
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.StripeCustomerId == evt.StripeCustomerId, ct);
            memberId = subscription?.MemberId;
        }

        _db.CashflowEntries.Add(new CashflowEntry
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Amount = evt.AmountPaid.Value / 100m,
            Currency = evt.Currency ?? "usd",
            Source = CashflowSource.StripeInvoice,
            Category = CashflowCategory.Membership,
            Description = $"Stripe invoice {evt.StripeInvoiceId}",
            ReferenceStripeObjectId = evt.StripeInvoiceId,
            OccurredAtUtc = DateTime.UtcNow,
            ReconciliationStatus = ReconciliationStatus.Reconciled,
        });

        await _db.SaveChangesAsync(ct);
    }

    private async Task HandleInvoicePaymentFailedAsync(StripeWebhookEvent evt, CancellationToken ct)
    {
        if (evt.StripeSubscriptionId is null) return;

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == evt.StripeSubscriptionId, ct);
        if (subscription is null) return;

        subscription.Status = SubscriptionStatus.PastDue;
        await _db.SaveChangesAsync(ct);
    }

    private static SubscriptionStatus MapStripeStatus(string? stripeStatus) => stripeStatus switch
    {
        "trialing" => SubscriptionStatus.Trialing,
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "canceled" => SubscriptionStatus.Canceled,
        "unpaid" => SubscriptionStatus.Unpaid,
        "incomplete" => SubscriptionStatus.Incomplete,
        "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
        _ => SubscriptionStatus.Incomplete,
    };
}
