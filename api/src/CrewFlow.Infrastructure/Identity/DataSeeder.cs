using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;
using CrewFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrewFlow.Infrastructure.Identity;

// Dev/demo convenience: seeds the four roles, one Admin account, and a handful of
// dance styles so the scaffold is immediately usable without manual setup.
public static class DataSeeder
{
    public const string DefaultAdminEmail = "admin@crewflow.dev";
    public const string DefaultAdminPassword = "ChangeMe123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        foreach (var roleName in CrewFlow.Domain.Identity.RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                EmailConfirmed = true,
                FirstName = "Studio",
                LastName = "Admin",
            };

            var result = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, CrewFlow.Domain.Identity.RoleNames.Admin);
            }
        }

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
}
