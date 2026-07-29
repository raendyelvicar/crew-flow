using CrewFlow.Domain.Cashflow;

namespace CrewFlow.Application.Cashflow;

public record CashflowEntryResponse(
    Guid Id,
    Guid? MemberId,
    string? MemberName,
    decimal Amount,
    string Currency,
    CashflowSource Source,
    CashflowCategory Category,
    string? Description,
    string? ReferenceStripeObjectId,
    DateTime OccurredAtUtc,
    ReconciliationStatus ReconciliationStatus);

public record CreateManualCashflowEntryRequest(
    Guid? MemberId,
    decimal Amount,
    string Currency,
    CashflowSource Source,
    CashflowCategory Category,
    string? Description,
    DateTime OccurredAtUtc);

public record CashflowSummaryResponse(
    decimal TotalIncome, decimal TotalRefunds, decimal NetAmount, int EntryCount, IReadOnlyDictionary<string, decimal> ByCategory);
