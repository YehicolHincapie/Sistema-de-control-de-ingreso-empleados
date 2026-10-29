using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.AccessPoints.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.AccessPoints.Queries.GetAccessPoints;

public sealed record GetAccessPointsQuery(bool? IsEnabled = null) : IQuery<IReadOnlyList<AccessPointDto>>;

public sealed class GetAccessPointsQueryHandler : IQueryHandler<GetAccessPointsQuery, IReadOnlyList<AccessPointDto>>
{
    private readonly IAccessPointRepository _accessPointRepository;

    public GetAccessPointsQueryHandler(IAccessPointRepository accessPointRepository)
    {
        _accessPointRepository = accessPointRepository;
    }

    public async Task<IReadOnlyList<AccessPointDto>> Handle(GetAccessPointsQuery request, CancellationToken cancellationToken)
    {
        var accessPoints = await _accessPointRepository.GetAllAsync(request.IsEnabled, cancellationToken);
        return accessPoints.Select(ap => ap.ToDto()).ToList();
    }
}
