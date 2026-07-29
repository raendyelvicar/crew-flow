using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;

namespace CrewFlow.Domain.Bookings;

// Application-layer rule: only members with an Attended booking on the occurrence may post one.
public class ClassReview
{
    public Guid Id { get; set; }

    public Guid ClassOccurrenceId { get; set; }
    public ClassOccurrence? ClassOccurrence { get; set; }

    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public Guid InstructorUserId { get; set; }
    public ApplicationUser? InstructorUser { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
