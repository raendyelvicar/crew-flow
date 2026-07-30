using CrewFlow.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class ClassTypeConfiguration : IEntityTypeConfiguration<ClassType>
{
    public void Configure(EntityTypeBuilder<ClassType> builder)
    {
        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasOne(a => a.ClassGenre)
            .WithMany()
            .HasForeignKey(a => a.ClassGenreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ClassType)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.ClassTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.HasOne(cs => cs.Activity)
            .WithMany(a => a.ClassSchedules)
            .HasForeignKey(cs => cs.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade: removing a staff account shouldn't silently delete classes.
        builder.HasOne(cs => cs.InstructorUser)
            .WithMany()
            .HasForeignKey(cs => cs.InstructorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClassOccurrenceConfiguration : IEntityTypeConfiguration<ClassOccurrence>
{
    public void Configure(EntityTypeBuilder<ClassOccurrence> builder)
    {
        builder.HasIndex(o => o.StartAtUtc);

        builder.HasOne(o => o.ClassSchedule)
            .WithMany(cs => cs.Occurrences)
            .HasForeignKey(o => o.ClassScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Activity)
            .WithMany()
            .HasForeignKey(o => o.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.InstructorUser)
            .WithMany()
            .HasForeignKey(o => o.InstructorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
