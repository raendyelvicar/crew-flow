using CrewFlow.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasIndex(m => m.Email).IsUnique();
        builder.HasIndex(m => m.UserId).IsUnique();

        builder.HasOne(m => m.User)
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DanceStyleConfiguration : IEntityTypeConfiguration<DanceStyle>
{
    public void Configure(EntityTypeBuilder<DanceStyle> builder)
    {
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class MemberDanceStyleConfiguration : IEntityTypeConfiguration<MemberDanceStyle>
{
    public void Configure(EntityTypeBuilder<MemberDanceStyle> builder)
    {
        builder.HasKey(mds => new { mds.MemberId, mds.DanceStyleId });

        builder.HasOne(mds => mds.Member)
            .WithMany(m => m.DanceStyles)
            .HasForeignKey(mds => mds.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mds => mds.DanceStyle)
            .WithMany(s => s.MemberDanceStyles)
            .HasForeignKey(mds => mds.DanceStyleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
