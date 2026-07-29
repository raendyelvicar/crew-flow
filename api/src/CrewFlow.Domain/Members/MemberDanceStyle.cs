namespace CrewFlow.Domain.Members;

public class MemberDanceStyle
{
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public Guid DanceStyleId { get; set; }
    public DanceStyle? DanceStyle { get; set; }

    public SkillLevel SkillLevel { get; set; }
}
