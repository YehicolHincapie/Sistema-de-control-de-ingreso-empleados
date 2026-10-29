using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que representa el correo electrónico corporativo o personal del empleado.
/// </summary>
public sealed partial class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress()
    {
        Value = string.Empty;
    }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El correo electrónico del empleado es obligatorio.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 150 || !EmailRegex().IsMatch(normalized))
            throw new DomainException("El formato del correo electrónico no es válido.");

        return new EmailAddress(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}

