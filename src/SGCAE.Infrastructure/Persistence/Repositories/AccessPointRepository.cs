using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;

namespace SGCAE.Infrastructure.Persistence.Repositories;

public sealed class AccessPointRepository : IAccessPointRepository
{
    private readonly ApplicationDbContext _context;

    public AccessPointRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccessPoints
            .FirstOrDefaultAsync(ap => ap.Id == id, cancellationToken);
    }

    public async Task<AccessPoint?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.AccessPoints
            .FirstOrDefaultAsync(ap => ap.Code == normalizedCode, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.AccessPoints
            .AnyAsync(ap => ap.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<AccessPoint>> GetAllAsync(bool? isEnabledFilter = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AccessPoints.AsNoTracking().AsQueryable();

        if (isEnabledFilter.HasValue)
            query = query.Where(ap => ap.IsEnabled == isEnabledFilter.Value);

        return await query
            .OrderBy(ap => ap.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AccessPoint accessPoint, CancellationToken cancellationToken = default)
    {
        await _context.AccessPoints.AddAsync(accessPoint, cancellationToken);
    }

    public void Update(AccessPoint accessPoint)
    {
        _context.AccessPoints.Update(accessPoint);
    }
}

