using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Employees.Commands.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid Id) : ICommand<EmployeeDto>;

/// <summary>
/// Desactiva al empleado y revoca sus credenciales vigentes o bloqueadas,
/// de modo que una persona desvinculada no conserve credenciales válidas.
/// </summary>
public sealed class DeactivateEmployeeCommandHandler : ICommandHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        ICredentialRepository credentialRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _credentialRepository = credentialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el empleado con ID '{request.Id}'.");

        employee.Deactivate();

        var credentials = await _credentialRepository.GetNonRevokedByEmployeeIdAsync(employee.Id, cancellationToken);
        foreach (var credential in credentials)
        {
            credential.Revoke("Desvinculación laboral del empleado");
            _credentialRepository.Update(credential);
        }

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.ToDto();
    }
}
