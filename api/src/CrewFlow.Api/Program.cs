using System.Text;
using System.Text.Json.Serialization;
using CrewFlow.Api.Common;
using CrewFlow.Application;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Application.Common.Security;
using CrewFlow.Domain.Identity;
using CrewFlow.Infrastructure;
using CrewFlow.Infrastructure.Identity;
using CrewFlow.Infrastructure.Options;
using CrewFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireRole(RoleNames.Admin));
    options.AddPolicy(PolicyNames.FinanceAccess, policy => policy.RequireRole(RoleNames.Admin, RoleNames.Finance));
    options.AddPolicy(PolicyNames.OperationalAccess, policy => policy.RequireRole(RoleNames.Admin, RoleNames.Operational));
    options.AddPolicy(PolicyNames.AnyStaff, policy => policy.RequireRole(RoleNames.StaffRoles));
    options.AddPolicy(PolicyNames.MemberOnly, policy => policy.RequireRole(RoleNames.Member));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("Default") ?? string.Empty;
builder.Services.AddHealthChecks().AddNpgSql(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCrewFlowExceptionHandling();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue<bool>("AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();

// Exposed for CrewFlow.Api.Tests' WebApplicationFactory<Program>.
public partial class Program
{
}
