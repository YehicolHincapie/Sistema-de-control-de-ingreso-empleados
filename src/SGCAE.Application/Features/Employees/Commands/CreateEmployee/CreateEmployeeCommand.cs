using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Employees.Commands.CreateEmployee;

public sealed record CreateEmployeeCommand(
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string Email,
    string Position,
    string Department) : ICommand<EmployeeDto>;

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("La identificación del empleado es obligatoria.")
            .MinimumLength(3).WithMessage("La identificación debe tener al menos 3 caracteres.")
            .MaximumLength(30).WithMessage("La identificación no puede exceder 30 caracteres.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Los nombres son obligatorios.")
            .MaximumLength(100).WithMessage("Los nombres no pueden exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El correo electrónico no puede exceder 150 caracteres.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("El cargo es obligatorio.")
            .MaximumLength(100).WithMessage("El cargo no puede exceder 100 caracteres.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("El área organizacional es obligatoria.")
            .MaximumLength(100).WithMessage("El área organizacional no puede exceder 100 caracteres.");
    }
}

public sealed class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var normalizedId = request.IdentificationNumber.Trim().ToUpperInvariant();
        if (await _employeeRepository.ExistsByIdentificationAsync(normalizedId, cancellationToken))
            throw new ConflictException($"Ya existe un empleado registrado con la identificación '{normalizedId}'.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _employeeRepository.ExistsByEmailAsync(normalizedEmail, null, cancellationToken))
            throw new ConflictException($"Ya existe un empleado registrado con el correo '{normalizedEmail}'.");

        var employee = Employee.Create(
            request.IdentificationNumber,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Position,
            request.Department);

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.ToDto();
    }
}
