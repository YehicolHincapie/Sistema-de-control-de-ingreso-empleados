using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Features.AccessControl.DTOs;

public sealed record AccessLogDto(
    Guid Id,
    string CredentialCodeUsed,
    Guid? CredentialId,
    Guid? EmployeeId,
    string? EmployeeFullName,
    string? EmployeeIdentification,
    Guid AccessPointId,
    string? AccessPointCode,
    string? AccessPointName,
    MovementType MovementType,
    string MovementTypeName,
    AccessValidationResult ValidationResult,
    string ValidationResultName,
    bool IsAuthorized,
    string? RejectionReason,
    DateTime TimestampUtc);

public static class AccessLogMappingExtensions
{
    public static AccessLogDto ToDto(
        this AccessLog accessLog,
        Employee? employee = null,
        AccessPoint? accessPoint = null)
    {
        var emp = employee ?? accessLog.Employee;
        var ap = accessPoint ?? accessLog.AccessPoint;

        return new AccessLogDto(
            accessLog.Id,
            accessLog.CredentialCodeUsed,
            accessLog.CredentialId,
            accessLog.EmployeeId,
            emp?.FullName,
            emp?.IdentificationNumber.Value,
            accessLog.AccessPointId,
            ap?.Code,
            ap?.Name,
            accessLog.MovementType,
            accessLog.MovementType.ToString(),
            accessLog.ValidationResult,
            accessLog.ValidationResult.ToString(),
            accessLog.ValidationResult == AccessValidationResult.Authorized,
            accessLog.RejectionReason,
            accessLog.TimestampUtc);
    }
}

