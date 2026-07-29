using CrewFlow.Domain.Members;

namespace CrewFlow.Application.Members;

public record MemberDanceStyleDto(Guid DanceStyleId, string DanceStyleName, SkillLevel SkillLevel);

public record MemberResponse(
    Guid Id,
    Guid? UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateOnly? DateOfBirth,
    MemberStatus Status,
    DateTime JoinedAtUtc,
    string? Bio,
    string? AvatarUrl,
    string? InstagramHandle,
    string? TikTokHandle,
    string? WebsiteUrl,
    bool IsProfilePublic,
    string? Notes,
    IReadOnlyList<MemberDanceStyleDto> DanceStyles);

public record MemberDirectoryEntry(
    Guid Id,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl,
    IReadOnlyList<MemberDanceStyleDto> DanceStyles);

public record CreateMemberRequest(string FirstName, string LastName, string Email, string? Phone, DateOnly? DateOfBirth);

public record UpdateMemberProfileRequest(
    string? Phone,
    DateOnly? DateOfBirth,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Bio,
    string? AvatarUrl,
    string? InstagramHandle,
    string? TikTokHandle,
    string? WebsiteUrl,
    bool? IsProfilePublic);

public record MemberDanceStyleInput(Guid DanceStyleId, SkillLevel SkillLevel);

public record SetMemberDanceStylesRequest(IReadOnlyList<MemberDanceStyleInput> Styles);

public record UpdateMemberStatusRequest(MemberStatus Status);

public record DanceStyleResponse(Guid Id, string Name, bool IsActive);

public record UpsertDanceStyleRequest(string Name, bool IsActive);
