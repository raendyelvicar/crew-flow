using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Members;

public class MemberService
{
    private readonly IAppDbContext _db;

    public MemberService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MemberResponse>> ListAsync(MemberStatus? status, CancellationToken ct = default)
    {
        var query = _db.Members.AsNoTracking()
            .Include(m => m.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(m => m.Status == status);
        }

        var members = await query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToListAsync(ct);
        return members.Select(MapToResponse).ToList();
    }

    public async Task<MemberResponse> GetByIdAsync(Guid memberId, CancellationToken ct = default)
    {
        var member = await FindTrackedAsync(memberId, ct);
        return MapToResponse(member);
    }

    public async Task<MemberResponse> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Members.AsNoTracking()
            .Include(m => m.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

        if (member is null)
        {
            throw new NotFoundException("Member profile", userId);
        }

        return MapToResponse(member);
    }

    public async Task<IReadOnlyList<MemberDirectoryEntry>> GetDirectoryAsync(
        Guid? danceStyleId, SkillLevel? skillLevel, CancellationToken ct = default)
    {
        var query = _db.Members.AsNoTracking()
            .Where(m => m.IsProfilePublic && m.Status == MemberStatus.Active)
            .Include(m => m.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .AsQueryable();

        if (danceStyleId is not null)
        {
            query = query.Where(m => m.DanceStyles.Any(ds => ds.DanceStyleId == danceStyleId
                && (skillLevel == null || ds.SkillLevel == skillLevel)));
        }

        var members = await query.OrderBy(m => m.FirstName).ToListAsync(ct);

        return members.Select(m => new MemberDirectoryEntry(
            m.Id, m.FirstName, m.LastName, m.Bio, m.AvatarUrl,
            m.DanceStyles.Select(ds => new MemberDanceStyleDto(ds.DanceStyleId, ds.DanceStyle!.Name, ds.SkillLevel)).ToList()))
            .ToList();
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Status = MemberStatus.Active,
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(member);
    }

    public async Task<MemberResponse> UpdateProfileAsync(Guid memberId, UpdateMemberProfileRequest request, CancellationToken ct = default)
    {
        var member = await FindTrackedAsync(memberId, ct);

        if (request.Phone is not null) member.Phone = request.Phone;
        if (request.DateOfBirth is not null) member.DateOfBirth = request.DateOfBirth;
        if (request.EmergencyContactName is not null) member.EmergencyContactName = request.EmergencyContactName;
        if (request.EmergencyContactPhone is not null) member.EmergencyContactPhone = request.EmergencyContactPhone;
        if (request.Bio is not null) member.Bio = request.Bio;
        if (request.AvatarUrl is not null) member.AvatarUrl = request.AvatarUrl;
        if (request.InstagramHandle is not null) member.InstagramHandle = request.InstagramHandle;
        if (request.TikTokHandle is not null) member.TikTokHandle = request.TikTokHandle;
        if (request.WebsiteUrl is not null) member.WebsiteUrl = request.WebsiteUrl;
        if (request.IsProfilePublic is not null) member.IsProfilePublic = request.IsProfilePublic.Value;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(memberId, ct);
    }

    public async Task<MemberResponse> SetDanceStylesAsync(Guid memberId, SetMemberDanceStylesRequest request, CancellationToken ct = default)
    {
        var member = await _db.Members
            .Include(m => m.DanceStyles)
            .FirstOrDefaultAsync(m => m.Id == memberId, ct)
            ?? throw new NotFoundException(nameof(Member), memberId);

        member.DanceStyles.Clear();
        foreach (var style in request.Styles)
        {
            member.DanceStyles.Add(new MemberDanceStyle
            {
                MemberId = memberId,
                DanceStyleId = style.DanceStyleId,
                SkillLevel = style.SkillLevel,
            });
        }

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(memberId, ct);
    }

    public async Task<MemberResponse> UpdateStatusAsync(Guid memberId, UpdateMemberStatusRequest request, CancellationToken ct = default)
    {
        var member = await FindTrackedAsync(memberId, ct);
        member.Status = request.Status;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(memberId, ct);
    }

    private async Task<Member> FindTrackedAsync(Guid memberId, CancellationToken ct)
    {
        return await _db.Members
            .Include(m => m.DanceStyles).ThenInclude(ds => ds.DanceStyle)
            .FirstOrDefaultAsync(m => m.Id == memberId, ct)
            ?? throw new NotFoundException(nameof(Member), memberId);
    }

    private static MemberResponse MapToResponse(Member member) => new(
        member.Id,
        member.UserId,
        member.FirstName,
        member.LastName,
        member.Email,
        member.Phone,
        member.DateOfBirth,
        member.Status,
        member.JoinedAtUtc,
        member.Bio,
        member.AvatarUrl,
        member.InstagramHandle,
        member.TikTokHandle,
        member.WebsiteUrl,
        member.IsProfilePublic,
        member.Notes,
        member.DanceStyles.Select(ds => new MemberDanceStyleDto(ds.DanceStyleId, ds.DanceStyle?.Name ?? string.Empty, ds.SkillLevel)).ToList());
}
