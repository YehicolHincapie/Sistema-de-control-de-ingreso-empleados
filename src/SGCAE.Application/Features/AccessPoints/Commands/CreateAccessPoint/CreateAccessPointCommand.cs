using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.AccessPoints.DTOs;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.AccessPoints.Commands.CreateAccessPoint;

public sealed record CreateAccessPointCommand(
    string Code,
    string Name,
    string Area,
    string? Description = null,
    bool IsRestrictedArea = false,
    bool IsEnabled = true) : ICommand<AccessPointDto>;

public sealed class CreateAccessPointCommandValidator : AbstractValidator<CreateAccessPointCommand>
{
    public CreateAccessPointCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del punto de acceso es obligatorio.")
            .MinimumLength(2).WithMessage("El código debe tener al menos 2 caracteres.")
            .MaximumLength(30).WithMessage("El código no puede exceder los 30 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del punto de acceso es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Area)
            .NotEmpty().WithMessage("El área física del punto de acceso es obligatoria.")
            .MaximumLength(100).WithMessage("El área no puede exceder los 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("La descripción no puede exceder los 250 caracteres.");
    }
}

public sealed class CreateAccessPointCommandHandler : ICommandHandler<CreateAccessPointCommand, AccessPointDto>
{
    private readonly IAccessPointRepository _accessPointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccessPointCommandHandler(IAccessPointRepository accessPointRepository, IUnitOfWork unitOfWork)
    {
        _accessPointRepository = accessPointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessPointDto> Handle(CreateAccessPointCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await _accessPointRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
            throw new ConflictException($"Ya existe un punto de acceso registrado con el código '{normalizedCode}'.");

        var accessPoint = AccessPoint.Create(
            request.Code,
            request.Name,
            request.Area,
            request.Description,
            request.IsRestrictedArea,
            request.IsEnabled);

        await _accessPointRepository.AddAsync(accessPoint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return accessPoint.ToDto();
    }
}
