using CrewFlow.Application.Common.Security;
using CrewFlow.Application.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/dance-styles")]
public class DanceStylesController : ControllerBase
{
    private readonly DanceStyleService _service;

    public DanceStylesController(DanceStyleService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<DanceStyleResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListAsync(activeOnly, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<DanceStyleResponse>> Create(UpsertDanceStyleRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<DanceStyleResponse>> Update(Guid id, UpsertDanceStyleRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));
}
