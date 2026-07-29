using System.Security.Claims;
using CrewFlow.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Common;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedException("Missing or invalid user identity.");
    }
}
