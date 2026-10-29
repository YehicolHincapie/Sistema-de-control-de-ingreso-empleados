using SGCAE.Domain.Entities;

namespace SGCAE.Domain.Repositories;

public interface ICredentialRepository
{
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Credential?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Credential>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<Credential?> GetActiveByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(Credential credential, CancellationToken cancellationToken = default);
    void Update(Credential credential);
}

