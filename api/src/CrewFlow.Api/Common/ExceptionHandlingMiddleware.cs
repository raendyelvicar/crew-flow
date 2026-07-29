using CrewFlow.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Common;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ValidationAppException => (StatusCodes.Status400BadRequest, "Validation failed"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = ex.Message,
                Instance = context.Request.Path,
            };

            if (ex is ValidationAppException validationEx)
            {
                problemDetails.Extensions["errors"] = validationEx.Errors;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCrewFlowExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
