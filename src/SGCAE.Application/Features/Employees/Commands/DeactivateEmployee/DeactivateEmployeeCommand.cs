using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Employees.Commands.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid Id) : ICommand<EmployeeDto>;

public sealed class DeactivateEmployeeCommandHandler : ICommandHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el empleado con ID '{request.Id}'.");

        employee.Deactivate();

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.ToDto();
    }
}

