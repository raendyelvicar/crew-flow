using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Members;

public class DanceStyleService
{
    private readonly IAppDbContext _db;

    public DanceStyleService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DanceStyleResponse>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.DanceStyles.AsNoTracking().AsQueryable();
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var styles = await query.OrderBy(s => s.Name).ToListAsync(ct);
        return styles.Select(s => new DanceStyleResponse(s.Id, s.Name, s.IsActive)).ToList();
    }

    public async Task<DanceStyleResponse> CreateAsync(UpsertDanceStyleRequest request, CancellationToken ct = default)
    {
        var style = new DanceStyle { Id = Guid.NewGuid(), Name = request.Name, IsActive = request.IsActive };
        _db.DanceStyles.Add(style);
        await _db.SaveChangesAsync(ct);
        return new DanceStyleResponse(style.Id, style.Name, style.IsActive);
    }

    public async Task<DanceStyleResponse> UpdateAsync(Guid id, UpsertDanceStyleRequest request, CancellationToken ct = default)
    {
        var style = await _db.DanceStyles.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(DanceStyle), id);

        style.Name = request.Name;
        style.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);

        return new DanceStyleResponse(style.Id, style.Name, style.IsActive);
    }
}
