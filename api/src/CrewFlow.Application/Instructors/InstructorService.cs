using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Instructors;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Instructors;

public class InstructorService
{
    private readonly IAppDbContext _db;

    public InstructorService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<InstructorProfileResponse>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.InstructorProfiles.AsNoTracking()
            .Include(i => i.User)
            .Include(i => i.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(i => i.IsActive);
        }

        var profiles = await query.ToListAsync(ct);
        return profiles.Select(MapToResponse).ToList();
    }

    public async Task<InstructorProfileResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var profile = await FindTrackedAsync(id, ct);
        return MapToResponse(profile);
    }

    public async Task<InstructorProfileResponse> UpsertAsync(UpsertInstructorProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _db.InstructorProfiles
            .Include(i => i.DanceStyles)
            .FirstOrDefaultAsync(i => i.UserId == request.UserId, ct);

        if (profile is null)
        {
            profile = new InstructorProfile { Id = Guid.NewGuid(), UserId = request.UserId };
            _db.InstructorProfiles.Add(profile);
        }

        profile.Bio = request.Bio;
        profile.AvatarUrl = request.AvatarUrl;
        profile.YearsExperience = request.YearsExperience;
        profile.InstagramHandle = request.InstagramHandle;
        profile.WebsiteUrl = request.WebsiteUrl;
        profile.IsActive = request.IsActive;

        profile.DanceStyles.Clear();
        foreach (var styleId in request.DanceStyleIds)
        {
            profile.DanceStyles.Add(new InstructorDanceStyle { InstructorProfileId = profile.Id, DanceStyleId = styleId });
        }

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(profile.Id, ct);
    }

    private async Task<InstructorProfile> FindTrackedAsync(Guid id, CancellationToken ct)
    {
        return await _db.InstructorProfiles
            .Include(i => i.User)
            .Include(i => i.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException(nameof(InstructorProfile), id);
    }

    private static InstructorProfileResponse MapToResponse(InstructorProfile profile) => new(
        profile.Id,
        profile.UserId,
        profile.User?.FirstName ?? string.Empty,
        profile.User?.LastName ?? string.Empty,
        profile.Bio,
        profile.AvatarUrl,
        profile.YearsExperience,
        profile.InstagramHandle,
        profile.WebsiteUrl,
        profile.IsActive,
        profile.DanceStyles.Select(ds => new InstructorDanceStyleDto(ds.DanceStyleId, ds.DanceStyle?.Name ?? string.Empty)).ToList());
}
