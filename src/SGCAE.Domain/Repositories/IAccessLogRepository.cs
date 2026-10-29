using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Repositories;

public interface IAccessLogRepository
{
    Task AddAsync(AccessLog accessLog, CancellationToken cancellationToken = default);

    Task<PagedResult<AccessLog>> GetPagedHistoryAsync(
        Guid? employeeId,
        Guid? accessPointId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        AccessValidationResult? validationResult,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
