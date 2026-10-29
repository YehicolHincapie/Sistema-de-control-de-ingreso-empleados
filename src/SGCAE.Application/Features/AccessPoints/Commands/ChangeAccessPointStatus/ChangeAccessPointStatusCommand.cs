using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.AccessPoints.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.AccessPoints.Commands.ChangeAccessPointStatus;

public sealed record ChangeAccessPointStatusCommand(
    Guid Id,
    bool IsEnabled) : ICommand<AccessPointDto>;

public sealed class ChangeAccessPointStatusCommandHandler : ICommandHandler<ChangeAccessPointStatusCommand, AccessPointDto>
{
    private readonly IAccessPointRepository _accessPointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeAccessPointStatusCommandHandler(IAccessPointRepository accessPointRepository, IUnitOfWork unitOfWork)
    {
        _accessPointRepository = accessPointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessPointDto> Handle(ChangeAccessPointStatusCommand request, CancellationToken cancellationToken)
    {
        var accessPoint = await _accessPointRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el punto de acceso con ID '{request.Id}'.");

        if (request.IsEnabled)
            accessPoint.Enable();
        else
            accessPoint.Disable();

        _accessPointRepository.Update(accessPoint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return accessPoint.ToDto();
    }
}
