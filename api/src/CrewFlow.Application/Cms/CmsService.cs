using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Cms;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Cms;

public class CmsService
{
    private readonly IAppDbContext _db;

    public CmsService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PageResponse>> ListAllAsync(CancellationToken ct = default)
    {
        var pages = await _db.Pages.AsNoTracking().Include(p => p.Sections).OrderBy(p => p.Slug).ToListAsync(ct);
        return pages.Select(Map).ToList();
    }

    public async Task<PageResponse?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var page = await _db.Pages.AsNoTracking()
            .Include(p => p.Sections.Where(s => s.IsVisible))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, ct);

        return page is null ? null : Map(page);
    }

    public async Task<PageResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var page = await _db.Pages.AsNoTracking().Include(p => p.Sections)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Page), id);

        return Map(page);
    }

    public async Task<PageResponse> CreatePageAsync(UpsertPageRequest request, Guid userId, CancellationToken ct = default)
    {
        var exists = await _db.Pages.AnyAsync(p => p.Slug == request.Slug, ct);
        if (exists)
        {
            throw new ConflictException($"A page with slug '{request.Slug}' already exists.");
        }

        var page = new Page
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Title = request.Title,
            IsPublished = false,
            UpdatedByUserId = userId,
        };

        _db.Pages.Add(page);
        await _db.SaveChangesAsync(ct);
        return Map(page);
    }

    public async Task<PageResponse> UpdatePageAsync(Guid id, UpsertPageRequest request, Guid userId, CancellationToken ct = default)
    {
        var page = await _db.Pages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Page), id);

        page.Slug = request.Slug;
        page.Title = request.Title;
        page.UpdatedAtUtc = DateTime.UtcNow;
        page.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);
        return Map(page);
    }

    public async Task<PageResponse> SetPublishStatusAsync(Guid id, bool isPublished, Guid userId, CancellationToken ct = default)
    {
        var page = await _db.Pages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Page), id);

        page.IsPublished = isPublished;
        page.PublishedAtUtc = isPublished ? DateTime.UtcNow : page.PublishedAtUtc;
        page.UpdatedAtUtc = DateTime.UtcNow;
        page.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);
        return Map(page);
    }

    public async Task<PageResponse> UpsertSectionAsync(Guid pageId, Guid? sectionId, UpsertPageSectionRequest request, CancellationToken ct = default)
    {
        var page = await _db.Pages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == pageId, ct)
            ?? throw new NotFoundException(nameof(Page), pageId);

        var section = sectionId is null ? null : page.Sections.FirstOrDefault(s => s.Id == sectionId);

        if (section is null)
        {
            section = new PageSection { Id = Guid.NewGuid(), PageId = pageId };
            _db.PageSections.Add(section);
        }

        section.SortOrder = request.SortOrder;
        section.SectionType = request.SectionType;
        section.ContentJson = request.ContentJson;
        section.IsVisible = request.IsVisible;

        page.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(pageId, ct);
    }

    public async Task DeleteSectionAsync(Guid pageId, Guid sectionId, CancellationToken ct = default)
    {
        var section = await _db.PageSections.FirstOrDefaultAsync(s => s.Id == sectionId && s.PageId == pageId, ct)
            ?? throw new NotFoundException(nameof(PageSection), sectionId);

        _db.PageSections.Remove(section);
        await _db.SaveChangesAsync(ct);
    }

    private static PageResponse Map(Page page) => new(
        page.Id,
        page.Slug,
        page.Title,
        page.IsPublished,
        page.PublishedAtUtc,
        page.UpdatedAtUtc,
        page.Sections.OrderBy(s => s.SortOrder)
            .Select(s => new PageSectionDto(s.Id, s.SortOrder, s.SectionType, s.ContentJson, s.IsVisible))
            .ToList());
}
