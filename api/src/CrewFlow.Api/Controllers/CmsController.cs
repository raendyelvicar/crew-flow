using CrewFlow.Api.Common;
using CrewFlow.Application.Cms;
using CrewFlow.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/cms")]
public class CmsController : ControllerBase
{
    private readonly CmsService _service;

    public CmsController(CmsService service)
    {
        _service = service;
    }

    [HttpGet("pages")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<IReadOnlyList<PageResponse>>> ListAll(CancellationToken ct)
        => Ok(await _service.ListAllAsync(ct));

    [HttpGet("pages/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<PageResponse>> GetPublished(string slug, CancellationToken ct)
    {
        var page = await _service.GetPublishedBySlugAsync(slug, ct);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpGet("pages/by-id/{id:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<PageResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost("pages")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<PageResponse>> Create(UpsertPageRequest request, CancellationToken ct)
        => Ok(await _service.CreatePageAsync(request, this.GetUserId(), ct));

    [HttpPut("pages/{id:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<PageResponse>> Update(Guid id, UpsertPageRequest request, CancellationToken ct)
        => Ok(await _service.UpdatePageAsync(id, request, this.GetUserId(), ct));

    [HttpPost("pages/{id:guid}/publish")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<PageResponse>> Publish(Guid id, [FromQuery] bool isPublished, CancellationToken ct)
        => Ok(await _service.SetPublishStatusAsync(id, isPublished, this.GetUserId(), ct));

    [HttpPut("pages/{pageId:guid}/sections/{sectionId:guid?}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<PageResponse>> UpsertSection(Guid pageId, Guid? sectionId, UpsertPageSectionRequest request, CancellationToken ct)
        => Ok(await _service.UpsertSectionAsync(pageId, sectionId, request, ct));

    [HttpDelete("pages/{pageId:guid}/sections/{sectionId:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<IActionResult> DeleteSection(Guid pageId, Guid sectionId, CancellationToken ct)
    {
        await _service.DeleteSectionAsync(pageId, sectionId, ct);
        return NoContent();
    }
}
