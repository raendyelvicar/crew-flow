using CrewFlow.Domain.Instructors;
using CrewFlow.Domain.Members;
using Microsoft.AspNetCore.Identity;

namespace CrewFlow.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Member? Member { get; set; }
    public InstructorProfile? InstructorProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
