using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que representa el código único de una credencial de acceso (RFID/Token).
/// </summary>
public sealed partial class CredentialCode : ValueObject
{
    public string Value { get; }

    private CredentialCode()
    {
        Value = string.Empty;
    }

    private CredentialCode(string value)
    {
        Value = value;
    }

    public static CredentialCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El código de la credencial es obligatorio.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length is < 4 or > 64)
            throw new DomainException("El código de la credencial debe tener entre 4 y 64 caracteres.");

        if (!ValidCodeRegex().IsMatch(normalized))
            throw new DomainException("El código de la credencial solo puede contener caracteres alfanuméricos y guiones.");

        return new CredentialCode(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z0-9\-]+$")]
    private static partial Regex ValidCodeRegex();
}
