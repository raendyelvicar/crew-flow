using CrewFlow.Application.Common.Exceptions;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CrewFlow.Application.Scheduling;

public record ClassTypeResponse(Guid Id, string Name, string? Description, bool IsActive);

public record UpsertClassTypeRequest(string Name, string? Description, bool IsActive);

public class ClassTypeService
{
    private readonly IAppDbContext _db;

    public ClassTypeService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClassTypeResponse>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = _db.ClassTypes.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(t => t.IsActive);

        var types = await query.OrderBy(t => t.Name).ToListAsync(ct);
        return types.Select(Map).ToList();
    }

    public async Task<ClassTypeResponse> CreateAsync(UpsertClassTypeRequest request, CancellationToken ct = default)
    {
        var type = new ClassType { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description, IsActive = request.IsActive };
        _db.ClassTypes.Add(type);
        await _db.SaveChangesAsync(ct);
        return Map(type);
    }

    public async Task<ClassTypeResponse> UpdateAsync(Guid id, UpsertClassTypeRequest request, CancellationToken ct = default)
    {
        var type = await _db.ClassTypes.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException(nameof(ClassType), id);

        type.Name = request.Name;
        type.Description = request.Description;
        type.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return Map(type);
    }

    private static ClassTypeResponse Map(ClassType t) => new(t.Id, t.Name, t.Description, t.IsActive);
}
