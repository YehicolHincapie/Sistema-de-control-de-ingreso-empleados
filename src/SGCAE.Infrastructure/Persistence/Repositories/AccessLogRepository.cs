using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;

namespace SGCAE.Infrastructure.Persistence.Repositories;

public sealed class AccessLogRepository : IAccessLogRepository
{
    private readonly ApplicationDbContext _context;

    public AccessLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AccessLog accessLog, CancellationToken cancellationToken = default)
    {
        await _context.AccessLogs.AddAsync(accessLog, cancellationToken);
    }

    public async Task<PagedResult<AccessLog>> GetPagedHistoryAsync(
        Guid? employeeId,
        Guid? accessPointId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        AccessValidationResult? validationResult,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AccessLogs
            .AsNoTracking()
            .Include(al => al.Employee)
            .Include(al => al.AccessPoint)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(al => al.EmployeeId == employeeId.Value);

        if (accessPointId.HasValue)
            query = query.Where(al => al.AccessPointId == accessPointId.Value);

        if (fromDateUtc.HasValue)
            query = query.Where(al => al.TimestampUtc >= fromDateUtc.Value);

        if (toDateUtc.HasValue)
            query = query.Where(al => al.TimestampUtc <= toDateUtc.Value);

        if (validationResult.HasValue)
            query = query.Where(al => al.ValidationResult == validationResult.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(al => al.TimestampUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AccessLog>(items, totalCount, pageNumber, pageSize);
    }
}

