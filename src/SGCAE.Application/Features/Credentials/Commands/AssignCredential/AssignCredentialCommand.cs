using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Commands.AssignCredential;

public sealed record AssignCredentialCommand(
    Guid EmployeeId,
    string Code) : ICommand<CredentialDto>;

public sealed class AssignCredentialCommandValidator : AbstractValidator<AssignCredentialCommand>
{
    public AssignCredentialCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("El identificador del empleado es obligatorio.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código único de la credencial es obligatorio.")
            .MinimumLength(4).WithMessage("El código de la credencial debe tener al menos 4 caracteres.")
            .MaximumLength(64).WithMessage("El código de la credencial no puede exceder los 64 caracteres.");
    }
}

public sealed class AssignCredentialCommandHandler : ICommandHandler<AssignCredentialCommand, CredentialDto>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignCredentialCommandHandler(
        ICredentialRepository credentialRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _credentialRepository = credentialRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CredentialDto> Handle(AssignCredentialCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el empleado con ID '{request.EmployeeId}'.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await _credentialRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
            throw new ConflictException($"Ya existe una credencial registrada con el código '{normalizedCode}'.");

        var activeCredential = await _credentialRepository.GetActiveByEmployeeIdAsync(employee.Id, cancellationToken);
        if (activeCredential is not null)
            throw new ConflictException($"El empleado ya posee una credencial vigente ('{activeCredential.Code.Value}'). Debe bloquearla o revocarla antes de asignar una nueva.");

        var credential = Credential.Issue(request.Code, employee);

        await _credentialRepository.AddAsync(credential, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return credential.ToDto(employee);
    }
}
