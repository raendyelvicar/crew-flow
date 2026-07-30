using CrewFlow.Api.Common;
using CrewFlow.Application.Bookings;
using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Security;
using CrewFlow.Application.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/class-types")]
public class ClassTypesController : ControllerBase
{
    private readonly ClassTypeService _service;

    public ClassTypesController(ClassTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ClassTypeResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListAsync(activeOnly, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ClassTypeResponse>> Create(UpsertClassTypeRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ClassTypeResponse>> Update(Guid id, UpsertClassTypeRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));
}

[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly ScheduleService _service;

    public ActivitiesController(ScheduleService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ActivityResponse>>> List([FromQuery] bool activeOnly, CancellationToken ct)
        => Ok(await _service.ListActivitiesAsync(activeOnly, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ActivityResponse>> Create(UpsertActivityRequest request, CancellationToken ct)
        => Ok(await _service.CreateActivityAsync(request, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ActivityResponse>> Update(Guid id, UpsertActivityRequest request, CancellationToken ct)
        => Ok(await _service.UpdateActivityAsync(id, request, ct));
}

[ApiController]
[Route("api/v1/class-schedules")]
public class ClassSchedulesController : ControllerBase
{
    private readonly ScheduleService _service;

    public ClassSchedulesController(ScheduleService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ClassScheduleResponse>>> List(CancellationToken ct)
        => Ok(await _service.ListClassSchedulesAsync(ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ClassScheduleResponse>> Create(CreateClassScheduleRequest request, CancellationToken ct)
        => Ok(await _service.CreateClassScheduleAsync(request, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ClassScheduleResponse>> Update(Guid id, UpdateClassScheduleRequest request, CancellationToken ct)
        => Ok(await _service.UpdateClassScheduleAsync(id, request, ct));

    [HttpPost("{id:guid}/generate-occurrences")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<IActionResult> GenerateOccurrences(Guid id, [FromQuery] int weeksAhead, CancellationToken ct)
    {
        var count = await _service.GenerateOccurrencesAsync(id, weeksAhead <= 0 ? 8 : weeksAhead, ct);
        return Ok(new { created = count });
    }
}

[ApiController]
[Route("api/v1/class-occurrences")]
public class ClassOccurrencesController : ControllerBase
{
    private readonly ScheduleService _scheduleService;
    private readonly BookingService _bookingService;

    public ClassOccurrencesController(ScheduleService scheduleService, BookingService bookingService)
    {
        _scheduleService = scheduleService;
        _bookingService = bookingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ClassOccurrenceResponse>>> List(
        [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, [FromQuery] Guid? activityId, CancellationToken ct)
        => Ok(await _scheduleService.ListOccurrencesAsync(fromUtc, toUtc, activityId, ct: ct));

    [HttpGet("mine")]
    [Authorize(Policy = PolicyNames.CoachAccess)]
    public async Task<ActionResult<IReadOnlyList<ClassOccurrenceResponse>>> Mine(
        [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
        => Ok(await _scheduleService.ListOccurrencesAsync(fromUtc, toUtc, activityId: null, instructorUserId: this.GetUserId(), ct: ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = PolicyNames.OperationalAccess)]
    public async Task<ActionResult<ClassOccurrenceResponse>> Update(Guid id, UpdateOccurrenceRequest request, CancellationToken ct)
        => Ok(await _scheduleService.UpdateOccurrenceAsync(id, request, ct));

    [HttpGet("{id:guid}/roster")]
    [Authorize(Policy = PolicyNames.OperationalOrCoach)]
    public async Task<ActionResult<IReadOnlyList<RosterEntryResponse>>> Roster(Guid id, CancellationToken ct)
    {
        await EnsureCanActOnOccurrenceAsync(id, ct);
        return Ok(await _bookingService.GetRosterAsync(id, ct));
    }

    private async Task EnsureCanActOnOccurrenceAsync(Guid occurrenceId, CancellationToken ct)
    {
        if (User.IsInRole(CrewFlow.Domain.Identity.RoleNames.Admin) || User.IsInRole(CrewFlow.Domain.Identity.RoleNames.Operational))
        {
            return;
        }

        var instructorId = await _scheduleService.GetOccurrenceInstructorIdAsync(occurrenceId, ct);
        if (instructorId != this.GetUserId())
        {
            throw new ForbiddenException("You can only manage classes you instruct.");
        }
    }
}
