using CrewFlow.Domain.Identity;

namespace CrewFlow.Domain.Instructors;

public class InstructorProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int? YearsExperience { get; set; }
    public string? InstagramHandle { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InstructorDanceStyle> DanceStyles { get; set; } = new List<InstructorDanceStyle>();
}
