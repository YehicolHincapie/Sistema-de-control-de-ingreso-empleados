using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Entidad/Agregado que representa una credencial física o digital de acceso asignada a un empleado (RF-02).
/// </summary>
public sealed class Credential : AggregateRoot
{
    public CredentialCode Code { get; private set; } = null!;
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public CredentialStatus Status { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? BlockedAtUtc { get; private set; }
    public string? BlockReason { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }

    private Credential()
    {
    }

    private Credential(Guid id, CredentialCode code, Guid employeeId) : base(id)
    {
        Code = code;
        EmployeeId = employeeId;
        Status = CredentialStatus.Active;
        IssuedAtUtc = DateTime.UtcNow;
    }

    public static Credential Issue(string code, Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (!employee.IsActive())
            throw new DomainException("No se puede asignar una credencial a un empleado cuyo vínculo laboral está inactivo.");

        var credentialCode = CredentialCode.Create(code);
        return new Credential(Guid.NewGuid(), credentialCode, employee.Id);
    }

    public void Block(string? reason = null)
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("No se puede bloquear una credencial que ya ha sido revocada definitivamente.");

        if (Status == CredentialStatus.Blocked)
            throw new DomainException("La credencial ya se encuentra bloqueada.");

        Status = CredentialStatus.Blocked;
        BlockedAtUtc = DateTime.UtcNow;
        BlockReason = string.IsNullOrWhiteSpace(reason) ? "Bloqueo administrativo o preventivo" : reason.Trim();
    }

    public void Unblock()
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("No se puede reactivar una credencial que ha sido revocada definitivamente.");

        if (Status == CredentialStatus.Active)
            throw new DomainException("La credencial ya se encuentra vigente/activa.");

        Status = CredentialStatus.Active;
        BlockedAtUtc = null;
        BlockReason = null;
    }

    public void Revoke(string? reason = null)
    {
        if (Status == CredentialStatus.Revoked)
            throw new DomainException("La credencial ya se encuentra revocada.");

        Status = CredentialStatus.Revoked;
        RevokedAtUtc = DateTime.UtcNow;
        RevocationReason = string.IsNullOrWhiteSpace(reason) ? "Revocación definitiva de credencial" : reason.Trim();
    }

    public bool IsUsable() => Status == CredentialStatus.Active;
}

