namespace CrewFlow.Domain.Cms;

public class PageSection
{
    public Guid Id { get; set; }

    public Guid PageId { get; set; }
    public Page? Page { get; set; }

    public int SortOrder { get; set; }
    public SectionType SectionType { get; set; }

    // Flexible per-section-type payload (jsonb column), e.g. { "heading": "...", "imageUrl": "..." }.
    // Validated against the section type's expected shape in the Application layer, not the DB.
    public string ContentJson { get; set; } = "{}";
    public bool IsVisible { get; set; } = true;
}
