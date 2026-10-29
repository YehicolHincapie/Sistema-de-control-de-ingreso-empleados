using SGCAE.Domain.Common;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Raíz de agregado que representa un punto de acceso físico de la organización (RF-06).
/// Ejemplos: Recepción, Parqueadero, Bodega, Área Restringida.
/// </summary>
public sealed class AccessPoint : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Area { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsRestrictedArea { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private AccessPoint()
    {
    }

    private AccessPoint(
        Guid id,
        string code,
        string name,
        string area,
        string? description,
        bool isRestrictedArea,
        bool isEnabled) : base(id)
    {
        Code = code;
        Name = name;
        Area = area;
        Description = description;
        IsRestrictedArea = isRestrictedArea;
        IsEnabled = isEnabled;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static AccessPoint Create(
        string code,
        string name,
        string area,
        string? description = null,
        bool isRestrictedArea = false,
        bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("El código del punto de acceso es obligatorio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del punto de acceso es obligatorio.");

        if (string.IsNullOrWhiteSpace(area))
            throw new DomainException("El área física del punto de acceso es obligatoria.");

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (normalizedCode.Length is < 2 or > 30)
            throw new DomainException("El código del punto de acceso debe tener entre 2 y 30 caracteres.");

        return new AccessPoint(
            Guid.NewGuid(),
            normalizedCode,
            name.Trim(),
            area.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            isRestrictedArea,
            isEnabled);
    }

    public void Update(string name, string area, string? description, bool isRestrictedArea)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del punto de acceso es obligatorio.");

        if (string.IsNullOrWhiteSpace(area))
            throw new DomainException("El área física del punto de acceso es obligatoria.");

        Name = name.Trim();
        Area = area.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsRestrictedArea = isRestrictedArea;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Enable()
    {
        if (IsEnabled)
            throw new DomainException("El punto de acceso ya se encuentra habilitado.");

        IsEnabled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Disable()
    {
        if (!IsEnabled)
            throw new DomainException("El punto de acceso ya se encuentra deshabilitado.");

        IsEnabled = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

