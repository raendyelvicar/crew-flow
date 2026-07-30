using System.Reflection;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Bookings;
using CrewFlow.Domain.Cashflow;
using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Instructors;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrewFlow.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Member> Members => Set<Member>();
    public DbSet<DanceStyle> DanceStyles => Set<DanceStyle>();
    public DbSet<MemberDanceStyle> MemberDanceStyles => Set<MemberDanceStyle>();

    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<InstructorDanceStyle> InstructorDanceStyles => Set<InstructorDanceStyle>();

    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<CreditPack> CreditPacks => Set<CreditPack>();
    public DbSet<CreditPackPurchase> CreditPackPurchases => Set<CreditPackPurchase>();

    public DbSet<CashflowEntry> CashflowEntries => Set<CashflowEntry>();
    public DbSet<ProcessedStripeEvent> ProcessedStripeEvents => Set<ProcessedStripeEvent>();

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ClassType> ClassTypes => Set<ClassType>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<ClassOccurrence> ClassOccurrences => Set<ClassOccurrence>();

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<ClassReview> ClassReviews => Set<ClassReview>();

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
