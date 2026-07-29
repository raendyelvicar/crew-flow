using CrewFlow.Domain.Members;

namespace CrewFlow.Domain.Instructors;

public class InstructorDanceStyle
{
    public Guid InstructorProfileId { get; set; }
    public InstructorProfile? InstructorProfile { get; set; }

    public Guid DanceStyleId { get; set; }
    public DanceStyle? DanceStyle { get; set; }
}
