using CrewFlow.Domain.Cashflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class CashflowEntryConfiguration : IEntityTypeConfiguration<CashflowEntry>
{
    public void Configure(EntityTypeBuilder<CashflowEntry> builder)
    {
        builder.Property(e => e.Amount).HasPrecision(12, 2);

        // Filtered unique index: guards Stripe webhook-retry idempotency without
        // constraining the many manual entries that have no Stripe object id.
        builder.HasIndex(e => e.ReferenceStripeObjectId)
            .IsUnique()
            .HasFilter("\"ReferenceStripeObjectId\" IS NOT NULL");

        builder.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.RecordedByUser)
            .WithMany()
            .HasForeignKey(e => e.RecordedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProcessedStripeEventConfiguration : IEntityTypeConfiguration<ProcessedStripeEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
    {
        builder.HasIndex(e => e.StripeEventId).IsUnique();
    }
}
