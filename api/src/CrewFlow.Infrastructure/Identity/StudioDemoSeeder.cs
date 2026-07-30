using CrewFlow.Application.Scheduling;
using CrewFlow.Domain.Billing;
using CrewFlow.Domain.Instructors;
using CrewFlow.Domain.Members;
using CrewFlow.Domain.Scheduling;
using CrewFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrewFlow.Infrastructure.Identity;

// Demo data modeled on a real Jakarta dance studio's actual class roster (provided by the
// studio owner), so the scaffold has a believable, non-trivial dataset out of the box.
// Prices are IDR (a zero-decimal currency) and are not yet synced to Stripe - an admin
// re-saving a plan/pack via the admin UI creates the corresponding Stripe Product/Price.
public static class StudioDemoSeeder
{
    private const string DemoPassword = "ChangeMe123!";
    private const string Timezone = "Asia/Jakarta";

    private sealed record CoachSeed(string FirstName, string LastName, string? Genre);

    private static readonly CoachSeed[] Coaches =
    [
        new("Anis", "Coach", "Nusantara"),
        new("Sandy", "Coach", "Contemporary"),
        new("Wira", "Coach", "Contemporary"),
        new("Reinhard", "Coach", "Ladies"),
        new("Katya", "Coach", "Choreo"),
        new("Hebuzz", "Coach", "Choreo"),
        new("Tasya", "Coach", "Hip-Hop"),
        new("Arielle", "Coach", "Choreo"),
        new("Efa", "Coach", "Contemporary"),
        new("Dustin", "Coach", "K-Pop"),
        new("Jemima", "Coach", "Jazz Funk"),
        new("Anah", "Coach", "K-Pop"),
        new("Cindy", "Coach", null),
        new("ICM01", "Instructor", null),
        new("ICM02", "Instructor", null),
    ];

    private static readonly string[] ClassTypeNames = ["Regular", "Open", "Kids", "ICM Course"];

    private sealed record ActivitySeed(string Name, string Genre, string Type, string Description, int Capacity);

    private static readonly ActivitySeed[] Activities =
    [
        new("Nusantara", "Nusantara", "Regular", "Regular weekly class.", 15),
        new("Contemporary", "Contemporary", "Regular", "Regular weekly class.", 15),
        new("Ladies", "Ladies", "Regular", "Regular weekly class.", 15),
        new("Choreo", "Choreo", "Regular", "Regular weekly choreography class.", 18),
        new("Choreo Open", "Choreo", "Open", "Open drop-in choreography session.", 20),
        new("ICM", "ICM", "ICM Course", "ICM course track.", 10),
        new("K-Pop", "K-Pop", "Open", "Open drop-in class.", 20),
        new("Hip-Hop", "Hip-Hop", "Open", "Open drop-in class.", 20),
        new("Junior Class", "Kids", "Kids", "Kids program.", 12),
        new("K-Pop Intensive", "K-Pop", "Kids", "Kids program.", 12),
        new("K-Pop Kids", "K-Pop", "Kids", "Kids program.", 12),
        new("Hip-Hop Intro", "Hip-Hop", "Kids", "Kids program.", 12),
        new("HOMiES", "Hip-Hop", "Kids", "Kids program.", 12),
        new("Hip Hop Lv.1", "Hip-Hop", "Kids", "Kids program.", 12),
    ];

    private sealed record ScheduleSeed(string ActivityName, string CoachFirstName, DayOfWeek Day, TimeOnly StartTime);

    private static readonly ScheduleSeed[] Schedules =
    [
        new("Nusantara", "Anis", DayOfWeek.Monday, new TimeOnly(18, 30)),
        new("Contemporary", "Sandy", DayOfWeek.Tuesday, new TimeOnly(20, 0)),
        new("Contemporary", "Wira", DayOfWeek.Thursday, new TimeOnly(18, 30)),
        new("Ladies", "Reinhard", DayOfWeek.Thursday, new TimeOnly(20, 0)),
        new("Choreo", "Katya", DayOfWeek.Friday, new TimeOnly(20, 0)),
        new("Choreo Open", "Arielle", DayOfWeek.Tuesday, new TimeOnly(9, 30)),
        new("ICM", "ICM01", DayOfWeek.Wednesday, new TimeOnly(18, 45)),
        new("ICM", "ICM02", DayOfWeek.Wednesday, new TimeOnly(20, 30)),
        new("K-Pop", "Anah", DayOfWeek.Friday, new TimeOnly(18, 30)),
        new("Hip-Hop", "Tasya", DayOfWeek.Saturday, new TimeOnly(12, 30)),
        new("K-Pop", "Dustin", DayOfWeek.Saturday, new TimeOnly(14, 0)),
        new("Choreo Open", "Hebuzz", DayOfWeek.Saturday, new TimeOnly(17, 0)),
        new("Junior Class", "Cindy", DayOfWeek.Tuesday, new TimeOnly(14, 0)),
        new("K-Pop Intensive", "Anah", DayOfWeek.Friday, new TimeOnly(15, 0)),
        new("K-Pop Kids", "Anah", DayOfWeek.Friday, new TimeOnly(17, 0)),
        new("Hip-Hop Intro", "Tasya", DayOfWeek.Saturday, new TimeOnly(10, 0)),
        new("HOMiES", "Tasya", DayOfWeek.Saturday, new TimeOnly(14, 30)),
        new("Hip Hop Lv.1", "Tasya", DayOfWeek.Saturday, new TimeOnly(16, 0)),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        // Idempotent: skip entirely once this studio's data has been seeded once.
        if (await db.Activities.AnyAsync(a => a.Name == "Nusantara"))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<CrewFlow.Domain.Identity.ApplicationUser>>();
        var scheduleService = services.GetRequiredService<ScheduleService>();

        var neededStyles = new[] { "Nusantara", "Ladies", "Choreo", "K-Pop", "Jazz Funk", "Contemporary Jazz", "ICM", "Kids" };
        foreach (var styleName in neededStyles)
        {
            if (!await db.DanceStyles.AnyAsync(s => s.Name == styleName))
            {
                db.DanceStyles.Add(new DanceStyle { Id = Guid.NewGuid(), Name = styleName });
            }
        }

        foreach (var typeName in ClassTypeNames)
        {
            if (!await db.ClassTypes.AnyAsync(t => t.Name == typeName))
            {
                db.ClassTypes.Add(new ClassType { Id = Guid.NewGuid(), Name = typeName });
            }
        }
        await db.SaveChangesAsync();

        var styleIdByName = await db.DanceStyles.ToDictionaryAsync(s => s.Name, s => s.Id);
        var typeIdByName = await db.ClassTypes.ToDictionaryAsync(t => t.Name, t => t.Id);

        var coachUserIdByFirstName = new Dictionary<string, Guid>();
        foreach (var coach in Coaches)
        {
            var email = $"{coach.FirstName.ToLowerInvariant()}@strdc.demo";
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new CrewFlow.Domain.Identity.ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = coach.FirstName,
                    LastName = coach.LastName,
                };
                await userManager.CreateAsync(user, DemoPassword);
                await userManager.AddToRoleAsync(user, CrewFlow.Domain.Identity.RoleNames.Operational);
            }

            coachUserIdByFirstName[coach.FirstName] = user.Id;

            if (coach.Genre is not null
                && styleIdByName.TryGetValue(coach.Genre, out var styleId)
                && !await db.InstructorProfiles.AnyAsync(p => p.UserId == user.Id))
            {
                var profile = new InstructorProfile { Id = Guid.NewGuid(), UserId = user.Id, IsActive = true };
                db.InstructorProfiles.Add(profile);
                db.InstructorDanceStyles.Add(new InstructorDanceStyle { InstructorProfileId = profile.Id, DanceStyleId = styleId });
            }
        }
        await db.SaveChangesAsync();

        var activityIdByName = new Dictionary<string, Guid>();
        foreach (var activity in Activities)
        {
            var entity = new Activity
            {
                Id = Guid.NewGuid(),
                Name = activity.Name,
                Description = activity.Description,
                ClassGenreId = styleIdByName[activity.Genre],
                ClassTypeId = typeIdByName[activity.Type],
                DefaultCapacity = activity.Capacity,
                DefaultDurationMinutes = 60,
                IsActive = true,
            };
            db.Activities.Add(entity);
            activityIdByName[activity.Name] = entity.Id;
        }
        await db.SaveChangesAsync();

        // Reuses ScheduleService (rather than inserting ClassSchedule rows directly) so the
        // existing, tested occurrence-generation logic runs for each schedule automatically.
        foreach (var schedule in Schedules)
        {
            var capacity = Activities.First(a => a.Name == schedule.ActivityName).Capacity;
            await scheduleService.CreateClassScheduleAsync(new CreateClassScheduleRequest(
                activityIdByName[schedule.ActivityName],
                coachUserIdByFirstName[schedule.CoachFirstName],
                schedule.Day,
                schedule.StartTime,
                60,
                capacity,
                Timezone,
                DateOnly.FromDateTime(DateTime.UtcNow),
                null));
        }

        db.MembershipPlans.AddRange(
            new MembershipPlan
            {
                Id = Guid.NewGuid(), Name = "Regular Class Membership", Description = "Unlimited regular weekly classes.",
                BillingInterval = BillingInterval.Monthly, PriceCents = 400000, Currency = "idr", IsActive = true, SortOrder = 0,
            },
            new MembershipPlan
            {
                Id = Guid.NewGuid(), Name = "Kids Class Membership", Description = "Unlimited kids classes.",
                BillingInterval = BillingInterval.Monthly, PriceCents = 460000, Currency = "idr", IsActive = true, SortOrder = 1,
            });

        db.CreditPacks.AddRange(
            new CreditPack { Id = Guid.NewGuid(), Name = "Open Class - 1 Visit", Description = "Single drop-in open class.", CreditCount = 1, PriceCents = 150000, Currency = "idr", IsActive = true },
            new CreditPack { Id = Guid.NewGuid(), Name = "Open Class - 4 Visits", Description = "Valid for 1 month.", CreditCount = 4, PriceCents = 500000, Currency = "idr", ExpiryDays = 30, IsActive = true },
            new CreditPack { Id = Guid.NewGuid(), Name = "Open Class - 8 Visits", Description = "Valid for 2 months.", CreditCount = 8, PriceCents = 900000, Currency = "idr", ExpiryDays = 60, IsActive = true },
            new CreditPack { Id = Guid.NewGuid(), Name = "Kids Class Trial", Description = "1x visit trial.", CreditCount = 1, PriceCents = 100000, Currency = "idr", IsActive = true });

        await db.SaveChangesAsync();
    }
}
