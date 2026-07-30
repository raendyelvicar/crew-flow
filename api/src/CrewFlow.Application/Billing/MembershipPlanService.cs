using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Billing;

public class MembershipPlanService
{
    private readonly IAppDbContext _db;
    private readonly IStripeService _stripe;

    public MembershipPlanService(IAppDbContext db, IStripeService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    public async Task<IReadOnlyList<MembershipPlanResponse>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.MembershipPlans.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(p => p.IsActive);

        var plans = await query.OrderBy(p => p.SortOrder).ToListAsync(ct);
        return plans.Select(Map).ToList();
    }

    public async Task<MembershipPlanResponse> CreateAsync(UpsertMembershipPlanRequest request, CancellationToken ct = default)
    {
        var interval = request.BillingInterval == BillingInterval.Monthly ? "month" : "year";
        var (productId, priceId) = await _stripe.UpsertPlanPriceAsync(
            request.Name, request.Description, request.PriceAmount, request.Currency, interval, null, ct);

        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BillingInterval = request.BillingInterval,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            StripeProductId = productId,
            StripePriceId = priceId,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
        };

        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return Map(plan);
    }

    public async Task<MembershipPlanResponse> UpdateAsync(Guid id, UpsertMembershipPlanRequest request, CancellationToken ct = default)
    {
        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(MembershipPlan), id);

        var interval = request.BillingInterval == BillingInterval.Monthly ? "month" : "year";
        var (productId, priceId) = await _stripe.UpsertPlanPriceAsync(
            request.Name, request.Description, request.PriceAmount, request.Currency, interval, plan.StripeProductId, ct);

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.BillingInterval = request.BillingInterval;
        plan.PriceAmount = request.PriceAmount;
        plan.Currency = request.Currency;
        plan.StripeProductId = productId;
        plan.StripePriceId = priceId;
        plan.IsActive = request.IsActive;
        plan.SortOrder = request.SortOrder;

        await _db.SaveChangesAsync(ct);
        return Map(plan);
    }

    private static MembershipPlanResponse Map(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.BillingInterval, p.PriceAmount, p.Currency, p.IsActive, p.SortOrder);
}
