using CrewFlow.Api.Common;
using CrewFlow.Application.Cashflow;
using CrewFlow.Application.Common.Security;
using CrewFlow.Domain.Cashflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/cashflow")]
[Authorize(Policy = PolicyNames.FinanceAccess)]
public class CashflowController : ControllerBase
{
    private readonly CashflowService _service;

    public CashflowController(CashflowService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CashflowEntryResponse>>> List(
        [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] CashflowSource? source, CancellationToken ct)
        => Ok(await _service.ListAsync(fromUtc, toUtc, source, ct));

    [HttpPost]
    public async Task<ActionResult<CashflowEntryResponse>> CreateManual(CreateManualCashflowEntryRequest request, CancellationToken ct)
        => Ok(await _service.CreateManualEntryAsync(request, this.GetUserId(), ct));

    [HttpGet("summary")]
    public async Task<ActionResult<CashflowSummaryResponse>> Summary(
        [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
        => Ok(await _service.GetSummaryAsync(fromUtc, toUtc, ct));
}
