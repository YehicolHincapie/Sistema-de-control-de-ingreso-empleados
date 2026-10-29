using SGCAE.Domain.Entities;

namespace SGCAE.Application.Features.AccessPoints.DTOs;

public sealed record AccessPointDto(
    Guid Id,
    string Code,
    string Name,
    string Area,
    string? Description,
    bool IsRestrictedArea,
    bool IsEnabled,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public static class AccessPointMappingExtensions
{
    public static AccessPointDto ToDto(this AccessPoint accessPoint) =>
        new(
            accessPoint.Id,
            accessPoint.Code,
            accessPoint.Name,
            accessPoint.Area,
            accessPoint.Description,
            accessPoint.IsRestrictedArea,
            accessPoint.IsEnabled,
            accessPoint.CreatedAtUtc,
            accessPoint.UpdatedAtUtc);
}

