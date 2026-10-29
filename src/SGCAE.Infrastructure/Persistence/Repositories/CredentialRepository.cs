using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Infrastructure.Persistence.Repositories;

public sealed class CredentialRepository : ICredentialRepository
{
    private readonly ApplicationDbContext _context;

    public CredentialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Credentials
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Credential?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length is < 4 or > 64)
            return null;

        CredentialCode codeVo;
        try
        {
            codeVo = CredentialCode.Create(normalized);
        }
        catch
        {
            return null;
        }

        return await _context.Credentials
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Code == codeVo, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeVo = CredentialCode.Create(code);
        return await _context.Credentials
            .AnyAsync(c => c.Code == codeVo, cancellationToken);
    }

    public async Task<IReadOnlyList<Credential>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Credentials
            .AsNoTracking()
            .Include(c => c.Employee)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.IssuedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Credential?> GetActiveByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Credentials
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(
                c => c.EmployeeId == employeeId && c.Status == CredentialStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Credential>> GetNonRevokedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Credentials
            .Where(c => c.EmployeeId == employeeId && c.Status != CredentialStatus.Revoked)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Credential credential, CancellationToken cancellationToken = default)
    {
        await _context.Credentials.AddAsync(credential, cancellationToken);
    }

    public void Update(Credential credential)
    {
        _context.Credentials.Update(credential);
    }
}

