using CrewFlow.Domain.Identity;

namespace CrewFlow.Domain.Members;

public class Member
{
    public Guid Id { get; set; }

    // Nullable: staff can create a front-desk member record before the person
    // ever creates a login. Linked once they self-register or an admin links an account.
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Community profile
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? InstagramHandle { get; set; }
    public string? TikTokHandle { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsProfilePublic { get; set; }

    public ICollection<MemberDanceStyle> DanceStyles { get; set; } = new List<MemberDanceStyle>();
}
