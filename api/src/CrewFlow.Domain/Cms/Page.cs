using CrewFlow.Domain.Identity;

namespace CrewFlow.Domain.Cms;

public class Page
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
    public ApplicationUser? UpdatedByUser { get; set; }

    public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
}
