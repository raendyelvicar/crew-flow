using CrewFlow.Domain.Identity;

namespace CrewFlow.Domain.Scheduling;

// Recurrence is modeled simply (day-of-week + local start time) rather than
// full iCal RRULE - covers "weekly yoga every Tuesday 6pm" without recurrence-parsing complexity.
public class ClassSchedule
{
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }
    public Activity? Activity { get; set; }

    public Guid InstructorUserId { get; set; }
    public ApplicationUser? InstructorUser { get; set; }

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTimeLocal { get; set; }
    public int DurationMinutes { get; set; }
    public int Capacity { get; set; }
    public string Timezone { get; set; } = "UTC";

    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClassOccurrence> Occurrences { get; set; } = new List<ClassOccurrence>();
}
