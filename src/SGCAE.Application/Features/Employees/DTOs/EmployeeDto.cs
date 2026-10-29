using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.Features.Employees.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Position,
    string Department,
    EmploymentStatus Status,
    string StatusName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc);

public static class EmployeeMappingExtensions
{
    public static EmployeeDto ToDto(this Employee employee) =>
        new(
            employee.Id,
            employee.IdentificationNumber.Value,
            employee.FirstName,
            employee.LastName,
            employee.FullName,
            employee.Email.Value,
            employee.Position,
            employee.Department,
            employee.Status,
            employee.Status.ToString(),
            employee.CreatedAtUtc,
            employee.UpdatedAtUtc,
            employee.DeactivatedAtUtc);
}

