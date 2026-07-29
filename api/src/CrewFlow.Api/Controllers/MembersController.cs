using CrewFlow.Api.Common;
using CrewFlow.Application.Common.Security;
using CrewFlow.Application.Members;
using CrewFlow.Domain.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/members")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly MemberService _memberService;

    public MembersController(MemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> List([FromQuery] MemberStatus? status, CancellationToken ct)
        => Ok(await _memberService.ListAsync(status, ct));

    [HttpGet("directory")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MemberDirectoryEntry>>> Directory(
        [FromQuery] Guid? danceStyleId, [FromQuery] SkillLevel? skillLevel, CancellationToken ct)
        => Ok(await _memberService.GetDirectoryAsync(danceStyleId, skillLevel, ct));

    [HttpGet("me")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<MemberResponse>> Me(CancellationToken ct)
        => Ok(await _memberService.GetByUserIdAsync(this.GetUserId(), ct));

    [HttpPatch("me")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<MemberResponse>> UpdateMe(UpdateMemberProfileRequest request, CancellationToken ct)
    {
        var me = await _memberService.GetByUserIdAsync(this.GetUserId(), ct);
        return Ok(await _memberService.UpdateProfileAsync(me.Id, request, ct));
    }

    [HttpPut("me/dance-styles")]
    [Authorize(Policy = PolicyNames.MemberOnly)]
    public async Task<ActionResult<MemberResponse>> SetMyDanceStyles(SetMemberDanceStylesRequest request, CancellationToken ct)
    {
        var me = await _memberService.GetByUserIdAsync(this.GetUserId(), ct);
        return Ok(await _memberService.SetDanceStylesAsync(me.Id, request, ct));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<MemberResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(await _memberService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<MemberResponse>> Create(CreateMemberRequest request, CancellationToken ct)
    {
        var result = await _memberService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<MemberResponse>> UpdateStatus(Guid id, UpdateMemberStatusRequest request, CancellationToken ct)
        => Ok(await _memberService.UpdateStatusAsync(id, request, ct));

    [HttpPut("{id:guid}/dance-styles")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<MemberResponse>> SetDanceStyles(Guid id, SetMemberDanceStylesRequest request, CancellationToken ct)
        => Ok(await _memberService.SetDanceStylesAsync(id, request, ct));
}
