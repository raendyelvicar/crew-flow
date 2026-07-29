using CrewFlow.Domain.Cms;

namespace CrewFlow.Application.Cms;

public record PageSectionDto(Guid Id, int SortOrder, SectionType SectionType, string ContentJson, bool IsVisible);

public record PageResponse(
    Guid Id, string Slug, string Title, bool IsPublished, DateTime? PublishedAtUtc, DateTime UpdatedAtUtc,
    IReadOnlyList<PageSectionDto> Sections);

public record UpsertPageRequest(string Slug, string Title);

public record UpsertPageSectionRequest(int SortOrder, SectionType SectionType, string ContentJson, bool IsVisible);
