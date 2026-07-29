namespace CrewFlow.Domain.Members;

public class DanceStyle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<MemberDanceStyle> MemberDanceStyles { get; set; } = new List<MemberDanceStyle>();
}
