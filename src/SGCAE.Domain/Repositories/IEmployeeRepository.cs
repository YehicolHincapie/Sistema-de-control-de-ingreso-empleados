using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdentificationAsync(string identificationNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdentificationAsync(string identificationNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeEmployeeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetAllAsync(EmploymentStatus? statusFilter = null, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    void Update(Employee employee);
}

