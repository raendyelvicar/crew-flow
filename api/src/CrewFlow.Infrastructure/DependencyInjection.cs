using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Identity;
using CrewFlow.Infrastructure.Options;
using CrewFlow.Infrastructure.Persistence;
using CrewFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrewFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing 'ConnectionStrings:Default' configuration value.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // AddIdentityCore (not the full AddIdentity) since this is a pure API with no
        // cookie-based sign-in - we issue our own JWTs and don't need the ASP.NET Core
        // Identity UI/cookie middleware that AddIdentity would otherwise wire up.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IExternalAuthVerifier, GoogleExternalAuthVerifier>();

        return services;
    }
}
