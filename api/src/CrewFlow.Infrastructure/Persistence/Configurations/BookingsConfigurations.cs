using CrewFlow.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrewFlow.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Not a DB-level unique constraint (would need to exclude Cancelled rows, which is
        // awkward against an int-backed enum) - duplicate-active-booking prevention is
        // enforced in BookingService. This index just speeds up the lookups it performs.
        builder.HasIndex(b => new { b.ClassOccurrenceId, b.MemberId });

        builder.HasOne(b => b.ClassOccurrence)
            .WithMany(o => o.Bookings)
            .HasForeignKey(b => b.ClassOccurrenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Member)
            .WithMany()
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.CreditPackPurchase)
            .WithMany()
            .HasForeignKey(b => b.CreditPackPurchaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Subscription)
            .WithMany()
            .HasForeignKey(b => b.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ClassReviewConfiguration : IEntityTypeConfiguration<ClassReview>
{
    public void Configure(EntityTypeBuilder<ClassReview> builder)
    {
        builder.HasIndex(r => new { r.ClassOccurrenceId, r.MemberId }).IsUnique();

        builder.HasOne(r => r.ClassOccurrence)
            .WithMany()
            .HasForeignKey(r => r.ClassOccurrenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Member)
            .WithMany()
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
