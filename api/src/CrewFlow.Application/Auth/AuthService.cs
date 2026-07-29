using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Identity;
using CrewFlow.Domain.Members;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Auth;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IExternalAuthVerifier _externalAuthVerifier;
    private readonly IAppDbContext _db;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IExternalAuthVerifier externalAuthVerifier,
        IAppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _externalAuthVerifier = externalAuthVerifier;
        _db = db;
    }

    public async Task<AuthTokensResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException(createResult.Errors.Select(e => e.Description));
        }

        await EnsureRoleAsync(RoleNames.Member);
        await _userManager.AddToRoleAsync(user, RoleNames.Member);

        _db.Members.Add(new Member
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Status = MemberStatus.Active,
        });
        await _db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthTokensResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthTokensResponse> ExternalLoginAsync(ExternalLoginRequest request, CancellationToken ct = default)
    {
        if (!string.Equals(request.Provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationAppException([$"Unsupported external provider '{request.Provider}'."]);
        }

        var externalInfo = await _externalAuthVerifier.VerifyGoogleIdTokenAsync(request.IdToken, ct);

        var user = await _userManager.FindByLoginAsync(externalInfo.Provider, externalInfo.ProviderUserId);

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(externalInfo.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = externalInfo.Email,
                    Email = externalInfo.Email,
                    EmailConfirmed = true,
                    FirstName = externalInfo.FirstName ?? string.Empty,
                    LastName = externalInfo.LastName ?? string.Empty,
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new ValidationAppException(createResult.Errors.Select(e => e.Description));
                }

                await EnsureRoleAsync(RoleNames.Member);
                await _userManager.AddToRoleAsync(user, RoleNames.Member);

                _db.Members.Add(new Member
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    Status = MemberStatus.Active,
                });
                await _db.SaveChangesAsync(ct);
            }

            var loginInfo = new UserLoginInfo(externalInfo.Provider, externalInfo.ProviderUserId, externalInfo.Provider);
            await _userManager.AddLoginAsync(user, loginInfo);
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthTokensResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        if (existingToken is null || !existingToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        existingToken.RevokedAtUtc = DateTime.UtcNow;

        return await IssueTokensAsync(user, ct, existingToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        if (existingToken is not null && existingToken.RevokedAtUtc is null)
        {
            existingToken.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var member = await _db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == user.Id, ct);

        return new CurrentUserResponse(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToList(), member?.Id);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }

    private async Task<AuthTokensResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct, RefreshToken? replacing = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
        var (plainRefreshToken, refreshEntity) = _tokenService.GenerateRefreshToken(user.Id);

        if (replacing is not null)
        {
            replacing.ReplacedByTokenHash = refreshEntity.TokenHash;
        }

        _db.RefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync(ct);

        return new AuthTokensResponse(accessToken.AccessToken, accessToken.ExpiresAtUtc, plainRefreshToken);
    }
}
