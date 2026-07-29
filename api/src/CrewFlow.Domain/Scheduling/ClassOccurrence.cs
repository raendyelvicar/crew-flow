using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Identity;

namespace CrewFlow.Domain.Scheduling;

// Materialized instance of a ClassSchedule on a specific date. Generated ahead of
// time (rolling horizon) rather than computed on the fly, so Bookings have a stable
// FK target and per-occurrence overrides (capacity, substitute instructor, cancellation) are possible.
public class ClassOccurrence
{
    public Guid Id { get; set; }

    public Guid ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }

    public Guid ActivityId { get; set; }
    public Activity? Activity { get; set; }

    public Guid InstructorUserId { get; set; }
    public ApplicationUser? InstructorUser { get; set; }

    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public int Capacity { get; set; }
    public OccurrenceStatus Status { get; set; } = OccurrenceStatus.Scheduled;
    public string? CancellationReason { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
