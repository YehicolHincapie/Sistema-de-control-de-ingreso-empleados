using SGCAE.Domain.Entities;

namespace SGCAE.Domain.Repositories;

public interface IAccessPointRepository
{
    Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccessPoint?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessPoint>> GetAllAsync(bool? isEnabledFilter = null, CancellationToken cancellationToken = default);
    Task AddAsync(AccessPoint accessPoint, CancellationToken cancellationToken = default);
    void Update(AccessPoint accessPoint);
}

