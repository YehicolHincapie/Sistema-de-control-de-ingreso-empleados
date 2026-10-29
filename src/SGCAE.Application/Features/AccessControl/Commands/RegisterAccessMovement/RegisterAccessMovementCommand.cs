using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.AccessControl.DTOs;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.AccessControl.Commands.RegisterAccessMovement;

public sealed record RegisterAccessMovementCommand(
    string CredentialCode,
    Guid AccessPointId,
    MovementType MovementType) : ICommand<AccessLogDto>;

public sealed class RegisterAccessMovementCommandValidator : AbstractValidator<RegisterAccessMovementCommand>
{
    public RegisterAccessMovementCommandValidator()
    {
        RuleFor(x => x.CredentialCode)
            .NotEmpty().WithMessage("El código de la credencial es obligatorio.")
            .MaximumLength(64).WithMessage("El código de la credencial no puede exceder los 64 caracteres.");

        RuleFor(x => x.AccessPointId)
            .NotEmpty().WithMessage("El identificador del punto de acceso es obligatorio.");

        RuleFor(x => x.MovementType)
            .IsInEnum().WithMessage("El tipo de movimiento debe ser Entrada (1) o Salida (2).");
    }
}

public sealed class RegisterAccessMovementCommandHandler : ICommandHandler<RegisterAccessMovementCommand, AccessLogDto>
{
    private readonly IAccessLogRepository _accessLogRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAccessPointRepository _accessPointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterAccessMovementCommandHandler(
        IAccessLogRepository accessLogRepository,
        ICredentialRepository credentialRepository,
        IEmployeeRepository employeeRepository,
        IAccessPointRepository accessPointRepository,
        IUnitOfWork unitOfWork)
    {
        _accessLogRepository = accessLogRepository;
        _credentialRepository = credentialRepository;
        _employeeRepository = employeeRepository;
        _accessPointRepository = accessPointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessLogDto> Handle(RegisterAccessMovementCommand request, CancellationToken cancellationToken)
    {
        var accessPoint = await _accessPointRepository.GetByIdAsync(request.AccessPointId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el punto de acceso con ID '{request.AccessPointId}'.");

        var normalizedCode = request.CredentialCode.Trim().ToUpperInvariant();
        var credential = await _credentialRepository.GetByCodeAsync(normalizedCode, cancellationToken);

        Employee? employee = null;
        if (credential is not null)
        {
            employee = credential.Employee
                ?? await _employeeRepository.GetByIdAsync(credential.EmployeeId, cancellationToken);
        }

        // Evaluar invariantes de negocio y construir el registro trazable (Autorizado o Rechazado)
        var accessLog = AccessLog.EvaluateAccessAttempt(
            normalizedCode,
            credential,
            employee,
            accessPoint,
            request.MovementType);

        await _accessLogRepository.AddAsync(accessLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return accessLog.ToDto(employee, accessPoint);
    }
}

