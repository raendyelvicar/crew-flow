using CrewFlow.Application.Auth;
using CrewFlow.Application.Billing;
using CrewFlow.Application.Bookings;
using CrewFlow.Application.Cashflow;
using CrewFlow.Application.Instructors;
using CrewFlow.Application.Members;
using CrewFlow.Application.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace CrewFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<MemberService>();
        services.AddScoped<DanceStyleService>();
        services.AddScoped<InstructorService>();
        services.AddScoped<ScheduleService>();
        services.AddScoped<ClassTypeService>();
        services.AddScoped<BookingService>();
        services.AddScoped<MembershipPlanService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<CreditPackService>();
        services.AddScoped<CashflowService>();
        services.AddScoped<StripeWebhookProcessingService>();

        return services;
    }
}
