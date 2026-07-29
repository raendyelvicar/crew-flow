using CrewFlow.Domain.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.HasOne(p => p.UpdatedByUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PageSectionConfiguration : IEntityTypeConfiguration<PageSection>
{
    public void Configure(EntityTypeBuilder<PageSection> builder)
    {
        builder.Property(s => s.ContentJson).HasColumnType("jsonb");

        builder.HasOne(s => s.Page)
            .WithMany(p => p.Sections)
            .HasForeignKey(s => s.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
