using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByIdentificationAsync(string identificationNumber, CancellationToken cancellationToken = default)
    {
        var idVo = IdentificationNumber.Create(identificationNumber);
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.IdentificationNumber == idVo, cancellationToken);
    }

    public async Task<bool> ExistsByIdentificationAsync(string identificationNumber, CancellationToken cancellationToken = default)
    {
        var idVo = IdentificationNumber.Create(identificationNumber);
        return await _context.Employees
            .AnyAsync(e => e.IdentificationNumber == idVo, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var emailVo = EmailAddress.Create(email);
        var query = _context.Employees.AsQueryable().Where(e => e.Email == emailVo);

        if (excludeEmployeeId.HasValue)
            query = query.Where(e => e.Id != excludeEmployeeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        EmploymentStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsNoTracking().AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(e => e.Status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(e =>
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.Department.Contains(term) ||
                e.Position.Contains(term));
        }

        return await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
    }
}

