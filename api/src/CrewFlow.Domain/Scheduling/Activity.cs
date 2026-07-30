using CrewFlow.Domain.Members;

namespace CrewFlow.Domain.Scheduling;

public class Activity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // The "Class Genre" (e.g. K-Pop, Contemporary, Nusantara) - reuses the same DanceStyle
    // lookup that members use to tag their own community profile, so there's a single
    // admin-managed genre list instead of two parallel ones.
    public Guid ClassGenreId { get; set; }
    public DanceStyle? ClassGenre { get; set; }

    public Guid ClassTypeId { get; set; }
    public ClassType? ClassType { get; set; }

    public int DefaultCapacity { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
