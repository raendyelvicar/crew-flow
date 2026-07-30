using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Billing;

public class SubscriptionService
{
    private readonly IAppDbContext _db;
    private readonly IStripeService _stripe;

    public SubscriptionService(IAppDbContext db, IStripeService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    public async Task<CheckoutSessionResponse> StartSubscriptionCheckoutAsync(CreateSubscriptionCheckoutRequest request, CancellationToken ct = default)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);
        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == request.MembershipPlanId, ct)
            ?? throw new NotFoundException(nameof(MembershipPlan), request.MembershipPlanId);

        if (plan.StripePriceId is null)
        {
            throw new ConflictException("This plan is not yet synced with Stripe.");
        }

        var nonTerminal = Subscription.NonTerminalStatuses;
        var hasActive = await _db.Subscriptions.AnyAsync(s => s.MemberId == member.Id && nonTerminal.Contains(s.Status), ct);
        if (hasActive)
        {
            throw new ConflictException("This member already has an active subscription.");
        }

        var customerId = await _stripe.GetOrCreateCustomerAsync(member.Id, member.Email, $"{member.FirstName} {member.LastName}", ct);

        var metadata = new Dictionary<string, string>
        {
            ["memberId"] = member.Id.ToString(),
            ["membershipPlanId"] = plan.Id.ToString(),
        };

        var session = await _stripe.CreateSubscriptionCheckoutSessionAsync(
            customerId, plan.StripePriceId, request.SuccessUrl, request.CancelUrl, metadata, ct);

        // Placeholder row; the webhook fills in the Stripe subscription id/status once checkout completes.
        _db.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StripeCustomerId = customerId,
            Status = SubscriptionStatus.Incomplete,
            CreditsRemainingThisPeriod = plan.CreditsPerPeriod,
        });
        await _db.SaveChangesAsync(ct);

        return new CheckoutSessionResponse(session.CheckoutUrl);
    }

    public async Task<IReadOnlyList<SubscriptionResponse>> ListAsync(Guid? memberId, CancellationToken ct = default)
    {
        var query = _db.Subscriptions.AsNoTracking().Include(s => s.MembershipPlan).AsQueryable();
        if (memberId is not null) query = query.Where(s => s.MemberId == memberId);

        var subs = await query.OrderByDescending(s => s.CreatedAtUtc).ToListAsync(ct);
        return subs.Select(Map).ToList();
    }

    public async Task<SubscriptionResponse?> GetActiveForMemberAsync(Guid memberId, CancellationToken ct = default)
    {
        var nonTerminal = Subscription.NonTerminalStatuses;
        var sub = await _db.Subscriptions.AsNoTracking().Include(s => s.MembershipPlan)
            .Where(s => s.MemberId == memberId && nonTerminal.Contains(s.Status))
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        return sub is null ? null : Map(sub);
    }

    public async Task CancelAsync(Guid subscriptionId, bool atPeriodEnd, CancellationToken ct = default)
    {
        var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId, ct)
            ?? throw new NotFoundException(nameof(Subscription), subscriptionId);

        if (sub.StripeSubscriptionId is null)
        {
            throw new ConflictException("This subscription is not yet linked to Stripe.");
        }

        await _stripe.CancelSubscriptionAsync(sub.StripeSubscriptionId, atPeriodEnd, ct);

        if (atPeriodEnd)
        {
            sub.CancelAtPeriodEnd = true;
        }
        else
        {
            sub.Status = SubscriptionStatus.Canceled;
            sub.CanceledAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static SubscriptionResponse Map(Subscription s) => new(
        s.Id, s.MemberId, s.MembershipPlanId, s.MembershipPlan?.Name ?? string.Empty, s.Status,
        s.CurrentPeriodStartUtc, s.CurrentPeriodEndUtc, s.CancelAtPeriodEnd,
        s.CreditsRemainingThisPeriod, s.MembershipPlan?.CreditsPerPeriod ?? 0);
}
