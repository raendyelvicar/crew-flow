using CrewFlow.Domain.Instructors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.HasIndex(i => i.UserId).IsUnique();

        builder.HasOne(i => i.User)
            .WithOne(u => u.InstructorProfile)
            .HasForeignKey<InstructorProfile>(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InstructorDanceStyleConfiguration : IEntityTypeConfiguration<InstructorDanceStyle>
{
    public void Configure(EntityTypeBuilder<InstructorDanceStyle> builder)
    {
        builder.HasKey(ids => new { ids.InstructorProfileId, ids.DanceStyleId });

        builder.HasOne(ids => ids.InstructorProfile)
            .WithMany(i => i.DanceStyles)
            .HasForeignKey(ids => ids.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ids => ids.DanceStyle)
            .WithMany()
            .HasForeignKey(ids => ids.DanceStyleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
