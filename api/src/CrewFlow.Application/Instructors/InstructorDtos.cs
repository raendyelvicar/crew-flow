namespace CrewFlow.Application.Instructors;

public record InstructorDanceStyleDto(Guid DanceStyleId, string DanceStyleName);

public record InstructorProfileResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl,
    int? YearsExperience,
    string? InstagramHandle,
    string? WebsiteUrl,
    bool IsActive,
    IReadOnlyList<InstructorDanceStyleDto> DanceStyles);

public record UpsertInstructorProfileRequest(
    Guid UserId,
    string? Bio,
    string? AvatarUrl,
    int? YearsExperience,
    string? InstagramHandle,
    string? WebsiteUrl,
    bool IsActive,
    IReadOnlyList<Guid> DanceStyleIds);
