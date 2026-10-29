using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que representa la identificación única del empleado (cédula/documento).
/// </summary>
public sealed partial class IdentificationNumber : ValueObject
{
    public string Value { get; }

    private IdentificationNumber()
    {
        Value = string.Empty;
    }

    private IdentificationNumber(string value)
    {
        Value = value;
    }

    public static IdentificationNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El número de identificación del empleado es obligatorio.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length is < 3 or > 30)
            throw new DomainException("El número de identificación debe tener entre 3 y 30 caracteres.");

        if (!ValidIdentificationRegex().IsMatch(normalized))
            throw new DomainException("El número de identificación solo puede contener letras, números y guiones.");

        return new IdentificationNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z0-9\-]+$")]
    private static partial Regex ValidIdentificationRegex();
}
