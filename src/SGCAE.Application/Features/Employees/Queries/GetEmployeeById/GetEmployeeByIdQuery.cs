using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Employees.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid Id) : IQuery<EmployeeDto>;

public sealed class GetEmployeeByIdQueryHandler : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el empleado con ID '{request.Id}'.");

        return employee.ToDto();
    }
}

