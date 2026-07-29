using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Scheduling;

public class ScheduleService
{
    private readonly IAppDbContext _db;

    public ScheduleService(IAppDbContext db)
    {
        _db = db;
    }

    // --- Activities ---

    public async Task<IReadOnlyList<ActivityResponse>> ListActivitiesAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.Activities.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(a => a.IsActive);

        var activities = await query.OrderBy(a => a.Name).ToListAsync(ct);
        return activities.Select(MapActivity).ToList();
    }

    public async Task<ActivityResponse> CreateActivityAsync(UpsertActivityRequest request, CancellationToken ct = default)
    {
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            DefaultCapacity = request.DefaultCapacity,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            IsActive = request.IsActive,
        };
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync(ct);
        return MapActivity(activity);
    }

    public async Task<ActivityResponse> UpdateActivityAsync(Guid id, UpsertActivityRequest request, CancellationToken ct = default)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException(nameof(Activity), id);

        activity.Name = request.Name;
        activity.Description = request.Description;
        activity.Category = request.Category;
        activity.DefaultCapacity = request.DefaultCapacity;
        activity.DefaultDurationMinutes = request.DefaultDurationMinutes;
        activity.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return MapActivity(activity);
    }

    // --- Class schedules (recurring definitions) ---

    public async Task<IReadOnlyList<ClassScheduleResponse>> ListClassSchedulesAsync(CancellationToken ct = default)
    {
        var schedules = await _db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.Activity)
            .Include(cs => cs.InstructorUser)
            .OrderBy(cs => cs.DayOfWeek).ThenBy(cs => cs.StartTimeLocal)
            .ToListAsync(ct);

        return schedules.Select(MapSchedule).ToList();
    }

    public async Task<ClassScheduleResponse> CreateClassScheduleAsync(CreateClassScheduleRequest request, CancellationToken ct = default)
    {
        var schedule = new ClassSchedule
        {
            Id = Guid.NewGuid(),
            ActivityId = request.ActivityId,
            InstructorUserId = request.InstructorUserId,
            DayOfWeek = request.DayOfWeek,
            StartTimeLocal = request.StartTimeLocal,
            DurationMinutes = request.DurationMinutes,
            Capacity = request.Capacity,
            Timezone = request.Timezone,
            EffectiveFromDate = request.EffectiveFromDate,
            EffectiveToDate = request.EffectiveToDate,
            IsActive = true,
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        await GenerateOccurrencesAsync(schedule.Id, weeksAhead: 8, ct);

        var reloaded = await _db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.Activity).Include(cs => cs.InstructorUser)
            .FirstAsync(cs => cs.Id == schedule.Id, ct);

        return MapSchedule(reloaded);
    }

    // Generates occurrences for a rolling horizon. Idempotent: skips any date/time that
    // already has an occurrence, so it's safe to call repeatedly (e.g. from a nightly job).
    public async Task<int> GenerateOccurrencesAsync(Guid classScheduleId, int weeksAhead, CancellationToken ct = default)
    {
        var schedule = await _db.ClassSchedules.FirstOrDefaultAsync(cs => cs.Id == classScheduleId, ct)
            ?? throw new NotFoundException(nameof(ClassSchedule), classScheduleId);

        var timeZone = ResolveTimeZone(schedule.Timezone);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = schedule.EffectiveFromDate > today ? schedule.EffectiveFromDate : today;
        var horizonEnd = today.AddDays(weeksAhead * 7);

        var existingStarts = (await _db.ClassOccurrences
            .Where(o => o.ClassScheduleId == classScheduleId)
            .Select(o => o.StartAtUtc)
            .ToListAsync(ct))
            .ToHashSet();

        var created = 0;
        for (var date = startDate; date <= horizonEnd; date = date.AddDays(1))
        {
            if (schedule.EffectiveToDate is not null && date > schedule.EffectiveToDate) break;
            if (date.DayOfWeek != schedule.DayOfWeek) continue;

            var localDateTime = date.ToDateTime(schedule.StartTimeLocal, DateTimeKind.Unspecified);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);

            if (existingStarts.Contains(startUtc)) continue;

            _db.ClassOccurrences.Add(new ClassOccurrence
            {
                Id = Guid.NewGuid(),
                ClassScheduleId = schedule.Id,
                ActivityId = schedule.ActivityId,
                InstructorUserId = schedule.InstructorUserId,
                StartAtUtc = startUtc,
                EndAtUtc = startUtc.AddMinutes(schedule.DurationMinutes),
                Capacity = schedule.Capacity,
                Status = OccurrenceStatus.Scheduled,
            });
            created++;
        }

        if (created > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return created;
    }

    private static TimeZoneInfo ResolveTimeZone(string timezone)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    // --- Occurrences ---

    public async Task<IReadOnlyList<ClassOccurrenceResponse>> ListOccurrencesAsync(
        DateTime fromUtc, DateTime toUtc, Guid? activityId, CancellationToken ct = default)
    {
        var query = _db.ClassOccurrences.AsNoTracking()
            .Include(o => o.Activity)
            .Include(o => o.InstructorUser)
            .Include(o => o.Bookings)
            .Where(o => o.StartAtUtc >= fromUtc && o.StartAtUtc <= toUtc);

        if (activityId is not null)
        {
            query = query.Where(o => o.ActivityId == activityId);
        }

        var occurrences = await query.OrderBy(o => o.StartAtUtc).ToListAsync(ct);
        return occurrences.Select(MapOccurrence).ToList();
    }

    public async Task<ClassOccurrenceResponse> UpdateOccurrenceAsync(Guid occurrenceId, UpdateOccurrenceRequest request, CancellationToken ct = default)
    {
        var occurrence = await _db.ClassOccurrences
            .Include(o => o.Activity).Include(o => o.InstructorUser).Include(o => o.Bookings)
            .FirstOrDefaultAsync(o => o.Id == occurrenceId, ct)
            ?? throw new NotFoundException(nameof(ClassOccurrence), occurrenceId);

        if (request.Capacity is not null) occurrence.Capacity = request.Capacity.Value;
        if (request.InstructorUserId is not null) occurrence.InstructorUserId = request.InstructorUserId.Value;
        if (request.Status is not null) occurrence.Status = request.Status.Value;
        if (request.CancellationReason is not null) occurrence.CancellationReason = request.CancellationReason;

        await _db.SaveChangesAsync(ct);
        return MapOccurrence(occurrence);
    }

    private static ActivityResponse MapActivity(Activity a) => new(
        a.Id, a.Name, a.Description, a.Category, a.DefaultCapacity, a.DefaultDurationMinutes, a.IsActive);

    private static ClassScheduleResponse MapSchedule(ClassSchedule cs) => new(
        cs.Id,
        cs.ActivityId,
        cs.Activity?.Name ?? string.Empty,
        cs.InstructorUserId,
        $"{cs.InstructorUser?.FirstName} {cs.InstructorUser?.LastName}".Trim(),
        cs.DayOfWeek,
        cs.StartTimeLocal,
        cs.DurationMinutes,
        cs.Capacity,
        cs.Timezone,
        cs.EffectiveFromDate,
        cs.EffectiveToDate,
        cs.IsActive);

    private static ClassOccurrenceResponse MapOccurrence(ClassOccurrence o) => new(
        o.Id,
        o.ClassScheduleId,
        o.ActivityId,
        o.Activity?.Name ?? string.Empty,
        o.InstructorUserId,
        $"{o.InstructorUser?.FirstName} {o.InstructorUser?.LastName}".Trim(),
        o.StartAtUtc,
        o.EndAtUtc,
        o.Capacity,
        o.Bookings.Count(b => b.Status == BookingStatus.Booked),
        o.Bookings.Count(b => b.Status == BookingStatus.Waitlisted),
        o.Status,
        o.CancellationReason);
}
