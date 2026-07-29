using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Cashflow;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Cashflow;

public class CashflowService
{
    private readonly IAppDbContext _db;

    public CashflowService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CashflowEntryResponse>> ListAsync(
        DateTime? fromUtc, DateTime? toUtc, CashflowSource? source, CancellationToken ct = default)
    {
        var query = _db.CashflowEntries.AsNoTracking().Include(e => e.Member).AsQueryable();

        if (fromUtc is not null) query = query.Where(e => e.OccurredAtUtc >= fromUtc);
        if (toUtc is not null) query = query.Where(e => e.OccurredAtUtc <= toUtc);
        if (source is not null) query = query.Where(e => e.Source == source);

        var entries = await query.OrderByDescending(e => e.OccurredAtUtc).ToListAsync(ct);
        return entries.Select(Map).ToList();
    }

    public async Task<CashflowEntryResponse> CreateManualEntryAsync(
        CreateManualCashflowEntryRequest request, Guid recordedByUserId, CancellationToken ct = default)
    {
        var entry = new CashflowEntry
        {
            Id = Guid.NewGuid(),
            MemberId = request.MemberId,
            Amount = request.Amount,
            Currency = request.Currency,
            Source = request.Source,
            Category = request.Category,
            Description = request.Description,
            RecordedByUserId = recordedByUserId,
            OccurredAtUtc = request.OccurredAtUtc,
            ReconciliationStatus = ReconciliationStatus.Unreconciled,
        };

        _db.CashflowEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        return Map(entry);
    }

    public async Task<CashflowSummaryResponse> GetSummaryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var entries = await _db.CashflowEntries.AsNoTracking()
            .Where(e => e.OccurredAtUtc >= fromUtc && e.OccurredAtUtc <= toUtc)
            .ToListAsync(ct);

        var totalIncome = entries.Where(e => e.Amount > 0).Sum(e => e.Amount);
        var totalRefunds = entries.Where(e => e.Amount < 0).Sum(e => e.Amount);
        var byCategory = entries
            .GroupBy(e => e.Category.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return new CashflowSummaryResponse(totalIncome, totalRefunds, totalIncome + totalRefunds, entries.Count, byCategory);
    }

    private static CashflowEntryResponse Map(CashflowEntry e) => new(
        e.Id,
        e.MemberId,
        e.Member is null ? null : $"{e.Member.FirstName} {e.Member.LastName}".Trim(),
        e.Amount,
        e.Currency,
        e.Source,
        e.Category,
        e.Description,
        e.ReferenceStripeObjectId,
        e.OccurredAtUtc,
        e.ReconciliationStatus);
}
