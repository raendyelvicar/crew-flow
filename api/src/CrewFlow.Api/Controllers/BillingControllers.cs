using CrewFlow.Api.Common;
using CrewFlow.Application.Billing;
using CrewFlow.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/membership-plans")]
public class MembershipPlansController : ControllerBase
{
    private readonly MembershipPlanService _service;

    public MembershipPlansController(MembershipPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MembershipPlanResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListAsync(activeOnly, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.FinanceAccess)]
    public async Task<ActionResult<MembershipPlanResponse>> Create(UpsertMembershipPlanRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.FinanceAccess)]
    public async Task<ActionResult<MembershipPlanResponse>> Update(Guid id, UpsertMembershipPlanRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));
}

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _service;
    private readonly CrewFlow.Application.Members.MemberService _memberService;

    public SubscriptionsController(SubscriptionService service, CrewFlow.Application.Members.MemberService memberService)
    {
        _service = service;
        _memberService = memberService;
    }

    [HttpPost("checkout")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<CheckoutSessionResponse>> StartCheckout(CreateSubscriptionCheckoutRequest request, CancellationToken ct)
        => Ok(await _service.StartSubscriptionCheckoutAsync(request, ct));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] bool atPeriodEnd, CancellationToken ct)
    {
        await _service.CancelAsync(id, atPeriodEnd, ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<SubscriptionResponse?>> Mine(CancellationToken ct)
    {
        var me = await _memberService.GetByUserIdAsync(this.GetUserId(), ct);
        return Ok(await _service.GetActiveForMemberAsync(me.Id, ct));
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.FinanceAccess)]
    public async Task<ActionResult<IReadOnlyList<SubscriptionResponse>>> List([FromQuery] Guid? memberId, CancellationToken ct)
        => Ok(await _service.ListAsync(memberId, ct));
}

[ApiController]
[Route("api/v1/credit-packs")]
public class CreditPacksController : ControllerBase
{
    private readonly CreditPackService _service;
    private readonly CrewFlow.Application.Members.MemberService _memberService;

    public CreditPacksController(CreditPackService service, CrewFlow.Application.Members.MemberService memberService)
    {
        _service = service;
        _memberService = memberService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CreditPackResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListAsync(activeOnly, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.FinanceAccess)]
    public async Task<ActionResult<CreditPackResponse>> Create(UpsertCreditPackRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpPost("checkout")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<CheckoutSessionResponse>> StartCheckout(CreateCreditPackCheckoutRequest request, CancellationToken ct)
        => Ok(await _service.StartCreditPackCheckoutAsync(request, ct));

    [HttpGet("purchases/me")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<IReadOnlyList<CreditPackPurchaseResponse>>> MyPurchases(CancellationToken ct)
    {
        var me = await _memberService.GetByUserIdAsync(this.GetUserId(), ct);
        return Ok(await _service.GetMemberPurchasesAsync(me.Id, ct));
    }
}
