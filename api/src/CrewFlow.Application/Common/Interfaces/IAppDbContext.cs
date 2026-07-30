using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Cashflow;
using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Instructors;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrewFlow.Application.Common.Interfaces;

// Implemented by Infrastructure's AppDbContext. Lets Application services depend on
// an abstraction instead of EF Core/Npgsql directly, per the Onion dependency rule.
public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Member> Members { get; }
    DbSet<DanceStyle> DanceStyles { get; }
    DbSet<MemberDanceStyle> MemberDanceStyles { get; }

    DbSet<InstructorProfile> InstructorProfiles { get; }
    DbSet<InstructorDanceStyle> InstructorDanceStyles { get; }

    DbSet<MembershipPlan> MembershipPlans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<CreditPack> CreditPacks { get; }
    DbSet<CreditPackPurchase> CreditPackPurchases { get; }

    DbSet<CashflowEntry> CashflowEntries { get; }
    DbSet<ProcessedStripeEvent> ProcessedStripeEvents { get; }

    DbSet<Activity> Activities { get; }
    DbSet<ClassType> ClassTypes { get; }
    DbSet<ClassSchedule> ClassSchedules { get; }
    DbSet<ClassOccurrence> ClassOccurrences { get; }

    DbSet<Booking> Bookings { get; }
    DbSet<ClassReview> ClassReviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
