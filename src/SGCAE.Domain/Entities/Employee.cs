using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Raíz de agregado que representa a un empleado de la organización (RF-01).
/// </summary>
public sealed class Employee : AggregateRoot
{
    private readonly List<Credential> _credentials = [];

    public IdentificationNumber IdentificationNumber { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public string Position { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public EmploymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    public IReadOnlyCollection<Credential> Credentials => _credentials.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Employee()
    {
    }

    private Employee(
        Guid id,
        IdentificationNumber identificationNumber,
        string firstName,
        string lastName,
        EmailAddress email,
        string position,
        string department) : base(id)
    {
        IdentificationNumber = identificationNumber;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Position = position;
        Department = department;
        Status = EmploymentStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Employee Create(
        string identificationNumber,
        string firstName,
        string lastName,
        string email,
        string position,
        string department)
    {
        ValidateRequiredText(firstName, "nombres", 100);
        ValidateRequiredText(lastName, "apellidos", 100);
        ValidateRequiredText(position, "cargo", 100);
        ValidateRequiredText(department, "área organizacional", 100);

        var idNumberVo = IdentificationNumber.Create(identificationNumber);
        var emailVo = EmailAddress.Create(email);

        return new Employee(
            Guid.NewGuid(),
            idNumberVo,
            firstName.Trim(),
            lastName.Trim(),
            emailVo,
            position.Trim(),
            department.Trim());
    }

    public void Update(
        string firstName,
        string lastName,
        string email,
        string position,
        string department)
    {
        ValidateRequiredText(firstName, "nombres", 100);
        ValidateRequiredText(lastName, "apellidos", 100);
        ValidateRequiredText(position, "cargo", 100);
        ValidateRequiredText(department, "área organizacional", 100);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = EmailAddress.Create(email);
        Position = position.Trim();
        Department = department.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EmploymentStatus.Inactive)
            throw new DomainException("El empleado ya se encuentra inactivo.");

        Status = EmploymentStatus.Inactive;
        DeactivatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == EmploymentStatus.Active)
            throw new DomainException("El empleado ya se encuentra activo.");

        Status = EmploymentStatus.Active;
        DeactivatedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsActive() => Status == EmploymentStatus.Active;

    private static void ValidateRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"El campo '{fieldName}' del empleado es obligatorio.");

        if (value.Trim().Length > maxLength)
            throw new DomainException($"El campo '{fieldName}' no puede exceder los {maxLength} caracteres.");
    }
}
