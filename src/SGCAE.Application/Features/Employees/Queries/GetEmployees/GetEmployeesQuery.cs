using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Employees.Queries.GetEmployees;

public sealed record GetEmployeesQuery(
    EmploymentStatus? Status = null,
    string? SearchTerm = null) : IQuery<IReadOnlyList<EmployeeDto>>;

public sealed class GetEmployeesQueryHandler : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAllAsync(request.Status, request.SearchTerm, cancellationToken);
        return employees.Select(e => e.ToDto()).ToList();
    }
}

