using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Raíz de agregado inmutable que registra un movimiento de entrada o salida (RF-03, RF-04).
/// Registra tanto accesos autorizados como rechazados para garantizar trazabilidad completa.
/// </summary>
public sealed class AccessLog : AggregateRoot
{
    public string CredentialCodeUsed { get; private set; } = string.Empty;
    public Guid? CredentialId { get; private set; }
    public Credential? Credential { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public Guid AccessPointId { get; private set; }
    public AccessPoint? AccessPoint { get; private set; }
    public MovementType MovementType { get; private set; }
    public AccessValidationResult ValidationResult { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    private AccessLog()
    {
    }

    private AccessLog(
        Guid id,
        string credentialCodeUsed,
        Guid? credentialId,
        Guid? employeeId,
        Guid accessPointId,
        MovementType movementType,
        AccessValidationResult validationResult,
        string? rejectionReason) : base(id)
    {
        CredentialCodeUsed = credentialCodeUsed;
        CredentialId = credentialId;
        EmployeeId = employeeId;
        AccessPointId = accessPointId;
        MovementType = movementType;
        ValidationResult = validationResult;
        RejectionReason = rejectionReason;
        TimestampUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Evalúa las reglas e invariantes de negocio para una solicitud de acceso (RF-03)
    /// y genera el registro trazable (Autorizado o Rechazado con motivo).
    /// </summary>
    public static AccessLog EvaluateAccessAttempt(
        string rawCredentialCode,
        Credential? credential,
        Employee? employee,
        AccessPoint accessPoint,
        MovementType movementType)
    {
        ArgumentNullException.ThrowIfNull(accessPoint);

        var normalizedCode = string.IsNullOrWhiteSpace(rawCredentialCode)
            ? "DESCONOCIDO"
            : rawCredentialCode.Trim().ToUpperInvariant();

        // 1. Validar que el punto de acceso esté habilitado (RF-06 / RF-03)
        if (!accessPoint.IsEnabled)
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                credential?.Id,
                employee?.Id ?? credential?.EmployeeId,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                "Punto de acceso inhabilitado por administración.");
        }

        // 2. Validar existencia de la credencial (RF-02 / RF-03)
        if (credential is null)
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                null,
                null,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                "Credencial no registrada en el sistema.");
        }

        // 3. Validar estado de la credencial (RF-02 / RF-03)
        if (credential.Status == CredentialStatus.Revoked)
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                credential.Id,
                employee?.Id ?? credential.EmployeeId,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                "Credencial revocada definitivamente.");
        }

        if (credential.Status == CredentialStatus.Blocked)
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                credential.Id,
                employee?.Id ?? credential.EmployeeId,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                "Credencial bloqueada temporalmente.");
        }

        // 4. Validar existencia y estado laboral del empleado (RF-01 / RF-03)
        if (employee is null)
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                credential.Id,
                credential.EmployeeId,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                "Empleado asociado a la credencial no encontrado.");
        }

        if (!employee.IsActive())
        {
            return new AccessLog(
                Guid.NewGuid(),
                normalizedCode,
                credential.Id,
                employee.Id,
                accessPoint.Id,
                movementType,
                AccessValidationResult.Rejected,
                $"Vínculo laboral del empleado no activo (Estado: {employee.Status}).");
        }

        // 5. Todas las condiciones superadas -> Acceso Autorizado
        return new AccessLog(
            Guid.NewGuid(),
            normalizedCode,
            credential.Id,
            employee.Id,
            accessPoint.Id,
            movementType,
            AccessValidationResult.Authorized,
            null);
    }
}

