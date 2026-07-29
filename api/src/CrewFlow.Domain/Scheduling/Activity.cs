namespace CrewFlow.Domain.Scheduling;

public class Activity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int DefaultCapacity { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
