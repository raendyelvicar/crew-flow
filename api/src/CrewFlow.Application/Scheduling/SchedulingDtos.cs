using CrewFlow.Domain.Scheduling;

namespace CrewFlow.Application.Scheduling;

public record ActivityResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid ClassGenreId,
    string ClassGenreName,
    Guid ClassTypeId,
    string ClassTypeName,
    int DefaultCapacity,
    int DefaultDurationMinutes,
    bool IsActive);

public record UpsertActivityRequest(
    string Name, string? Description, Guid ClassGenreId, Guid ClassTypeId, int DefaultCapacity, int DefaultDurationMinutes, bool IsActive);

public record ClassScheduleResponse(
    Guid Id,
    Guid ActivityId,
    string ActivityName,
    Guid InstructorUserId,
    string InstructorName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTimeLocal,
    int DurationMinutes,
    int Capacity,
    string Timezone,
    DateOnly EffectiveFromDate,
    DateOnly? EffectiveToDate,
    bool IsActive);

public record CreateClassScheduleRequest(
    Guid ActivityId,
    Guid InstructorUserId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTimeLocal,
    int DurationMinutes,
    int Capacity,
    string Timezone,
    DateOnly EffectiveFromDate,
    DateOnly? EffectiveToDate);

public record ClassOccurrenceResponse(
    Guid Id,
    Guid ClassScheduleId,
    Guid ActivityId,
    string ActivityName,
    Guid InstructorUserId,
    string InstructorName,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    int Capacity,
    int BookedCount,
    int WaitlistCount,
    OccurrenceStatus Status,
    string? CancellationReason);

public record UpdateOccurrenceRequest(int? Capacity, Guid? InstructorUserId, OccurrenceStatus? Status, string? CancellationReason);

public record UpdateClassScheduleRequest(
    Guid InstructorUserId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTimeLocal,
    int DurationMinutes,
    int Capacity,
    string Timezone,
    DateOnly EffectiveFromDate,
    DateOnly? EffectiveToDate,
    bool IsActive);
