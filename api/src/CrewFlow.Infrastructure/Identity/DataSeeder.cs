using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;
using CrewFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrewFlow.Infrastructure.Identity;

// Dev/demo convenience: seeds every role, one demo account per role, and a
// handful of dance styles so the scaffold is immediately usable without manual setup.
public static class DataSeeder
{
    public const string DefaultAdminEmail = "admin@crewflow.dev";
    public const string DefaultFinanceEmail = "finance@crewflow.dev";
    public const string DefaultOperationalEmail = "operations@crewflow.dev";
    public const string DefaultMemberEmail = "member@crewflow.dev";
    public const string DefaultStaffPassword = "ChangeMe123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        await EnsureStaffUserAsync(userManager, DefaultAdminEmail, "Studio", "Admin", RoleNames.Admin);
        await EnsureStaffUserAsync(userManager, DefaultFinanceEmail, "Studio", "Finance", RoleNames.Finance);
        await EnsureStaffUserAsync(userManager, DefaultOperationalEmail, "Studio", "Operations", RoleNames.Operational);
        await EnsureMemberUserAsync(userManager, db, DefaultMemberEmail, "Demo", "Member");

        if (!await db.DanceStyles.AnyAsync())
        {
            db.DanceStyles.AddRange(
                new DanceStyle { Id = Guid.NewGuid(), Name = "Salsa" },
                new DanceStyle { Id = Guid.NewGuid(), Name = "Bachata" },
                new DanceStyle { Id = Guid.NewGuid(), Name = "Hip-Hop" },
                new DanceStyle { Id = Guid.NewGuid(), Name = "Contemporary" },
                new DanceStyle { Id = Guid.NewGuid(), Name = "Ballet" });

            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureStaffUserAsync(
        UserManager<ApplicationUser> userManager, string email, string firstName, string lastName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
        };

        var result = await userManager.CreateAsync(user, DefaultStaffPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task EnsureMemberUserAsync(
        UserManager<ApplicationUser> userManager, AppDbContext db, string email, string firstName, string lastName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
        };

        var result = await userManager.CreateAsync(user, DefaultStaffPassword);
        if (!result.Succeeded)
        {
            return;
        }

        await userManager.AddToRoleAsync(user, RoleNames.Member);

        db.Members.Add(new Member
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Status = MemberStatus.Active,
        });
        await db.SaveChangesAsync();
    }
}
