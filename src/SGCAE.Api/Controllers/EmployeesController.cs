using MediatR;
using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Application.Features.Credentials.Queries.GetCredentialsByEmployee;
using SGCAE.Application.Features.Employees.Commands.CreateEmployee;
using SGCAE.Application.Features.Employees.Commands.DeactivateEmployee;
using SGCAE.Application.Features.Employees.Commands.UpdateEmployee;
using SGCAE.Application.Features.Employees.DTOs;
using SGCAE.Application.Features.Employees.Queries.GetEmployeeById;
using SGCAE.Application.Features.Employees.Queries.GetEmployees;
using SGCAE.Domain.Enums;

namespace SGCAE.Api.Controllers;

/// <summary>
/// Endpoints para la gestión de empleados (RF-01).
/// </summary>
[ApiController]
[Route("api/v1/employees")]
[Produces("application/json")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Consulta el listado de empleados con filtro opcional por estado laboral o término de búsqueda (RF-01).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EmploymentStatus? status,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEmployeesQuery(status, searchTerm), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta la información detallada de un empleado por su ID (RF-01).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta las credenciales asignadas a un empleado específico (RF-02).
    /// </summary>
    [HttpGet("{id:guid}/credentials")]
    [ProducesResponseType(typeof(IReadOnlyList<CredentialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeCredentials(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCredentialsByEmployeeQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo empleado en el sistema con estado laboral Activo (RF-01).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza la información personal y organizacional de un empleado (RF-01).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Position,
            request.Department);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Desactiva el vínculo laboral de un empleado impidiendo futuros accesos (RF-01).
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateEmployeeCommand(id), cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Position,
    string Department);

