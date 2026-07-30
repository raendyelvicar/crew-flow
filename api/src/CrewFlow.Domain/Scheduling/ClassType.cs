namespace CrewFlow.Domain.Scheduling;

// e.g. Regular, Open, Kids, ICM Course - admin-managed, independent of the class's genre.
public class ClassType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
