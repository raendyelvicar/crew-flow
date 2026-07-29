using CrewFlow.Api.Common;
using CrewFlow.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrewFlow.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokensResponse>> Register(RegisterRequest request, CancellationToken ct)
        => Ok(await _authService.RegisterAsync(request, ct));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokensResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await _authService.LoginAsync(request, ct));

    [HttpPost("external")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokensResponse>> ExternalLogin(ExternalLoginRequest request, CancellationToken ct)
        => Ok(await _authService.ExternalLoginAsync(request, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokensResponse>> Refresh(RefreshRequest request, CancellationToken ct)
        => Ok(await _authService.RefreshAsync(request, ct));

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken ct)
        => Ok(await _authService.GetCurrentUserAsync(this.GetUserId(), ct));
}
