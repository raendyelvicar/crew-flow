using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Billing;

public class CreditPackService
{
    private readonly IAppDbContext _db;
    private readonly IStripeService _stripe;

    public CreditPackService(IAppDbContext db, IStripeService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    public async Task<IReadOnlyList<CreditPackResponse>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.CreditPacks.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(p => p.IsActive);

        var packs = await query.ToListAsync(ct);
        return packs.Select(Map).ToList();
    }

    public async Task<CreditPackResponse> CreateAsync(UpsertCreditPackRequest request, CancellationToken ct = default)
    {
        var (productId, priceId) = await _stripe.UpsertOneTimePriceAsync(
            request.Name, request.Description, request.PriceAmount, request.Currency, null, ct);

        var pack = new CreditPack
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreditCount = request.CreditCount,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            StripeProductId = productId,
            StripePriceId = priceId,
            ExpiryDays = request.ExpiryDays,
            IsActive = request.IsActive,
        };

        _db.CreditPacks.Add(pack);
        await _db.SaveChangesAsync(ct);
        return Map(pack);
    }

    public async Task<CreditPackResponse> UpdateAsync(Guid id, UpsertCreditPackRequest request, CancellationToken ct = default)
    {
        var pack = await _db.CreditPacks.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(CreditPack), id);

        var (productId, priceId) = await _stripe.UpsertOneTimePriceAsync(
            request.Name, request.Description, request.PriceAmount, request.Currency, pack.StripeProductId, ct);

        pack.Name = request.Name;
        pack.Description = request.Description;
        pack.CreditCount = request.CreditCount;
        pack.PriceAmount = request.PriceAmount;
        pack.Currency = request.Currency;
        pack.StripeProductId = productId;
        pack.StripePriceId = priceId;
        pack.ExpiryDays = request.ExpiryDays;
        pack.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return Map(pack);
    }

    public async Task<CheckoutSessionResponse> StartCreditPackCheckoutAsync(CreateCreditPackCheckoutRequest request, CancellationToken ct = default)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);
        var pack = await _db.CreditPacks.FirstOrDefaultAsync(p => p.Id == request.CreditPackId, ct)
            ?? throw new NotFoundException(nameof(CreditPack), request.CreditPackId);

        if (pack.StripePriceId is null)
        {
            throw new ConflictException("This credit pack is not yet synced with Stripe.");
        }

        var customerId = await _stripe.GetOrCreateCustomerAsync(member.Id, member.Email, $"{member.FirstName} {member.LastName}", ct);

        var metadata = new Dictionary<string, string>
        {
            ["memberId"] = member.Id.ToString(),
            ["creditPackId"] = pack.Id.ToString(),
        };

        var session = await _stripe.CreateCreditPackCheckoutSessionAsync(
            customerId, pack.StripePriceId, request.SuccessUrl, request.CancelUrl, metadata, ct);

        return new CheckoutSessionResponse(session.CheckoutUrl);
    }

    public async Task<IReadOnlyList<CreditPackPurchaseResponse>> GetMemberPurchasesAsync(Guid memberId, CancellationToken ct = default)
    {
        var purchases = await _db.CreditPackPurchases.AsNoTracking()
            .Include(p => p.CreditPack)
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.PurchasedAtUtc)
            .ToListAsync(ct);

        return purchases.Select(p => new CreditPackPurchaseResponse(
            p.Id, p.MemberId, p.CreditPackId, p.CreditPack?.Name ?? string.Empty, p.CreditsRemaining,
            p.PurchasedAtUtc, p.ExpiresAtUtc, p.Status)).ToList();
    }

    private static CreditPackResponse Map(CreditPack p) => new(
        p.Id, p.Name, p.Description, p.CreditCount, p.PriceAmount, p.Currency, p.ExpiryDays, p.IsActive);
}
