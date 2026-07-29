using CrewFlow.Application.Common.Security;
using CrewFlow.Application.Instructors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly InstructorService _service;

    public InstructorsController(InstructorService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<InstructorProfileResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListAsync(activeOnly, ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<InstructorProfileResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPut]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<InstructorProfileResponse>> Upsert(UpsertInstructorProfileRequest request, CancellationToken ct)
        => Ok(await _service.UpsertAsync(request, ct));
}
