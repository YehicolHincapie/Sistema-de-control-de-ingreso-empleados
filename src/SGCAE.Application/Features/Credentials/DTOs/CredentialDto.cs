using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Features.Credentials.DTOs;

public sealed record CredentialDto(
    Guid Id,
    string Code,
    Guid EmployeeId,
    string? EmployeeFullName,
    string? EmployeeIdentification,
    CredentialStatus Status,
    string StatusName,
    bool IsUsable,
    DateTime IssuedAtUtc,
    DateTime? BlockedAtUtc,
    string? BlockReason,
    DateTime? RevokedAtUtc,
    string? RevocationReason);

public static class CredentialMappingExtensions
{
    public static CredentialDto ToDto(this Credential credential, Employee? employee = null)
    {
        var emp = employee ?? credential.Employee;
        return new CredentialDto(
            credential.Id,
            credential.Code.Value,
            credential.EmployeeId,
            emp?.FullName,
            emp?.IdentificationNumber.Value,
            credential.Status,
            credential.Status.ToString(),
            credential.IsUsable(),
            credential.IssuedAtUtc,
            credential.BlockedAtUtc,
            credential.BlockReason,
            credential.RevokedAtUtc,
            credential.RevocationReason);
    }
}
