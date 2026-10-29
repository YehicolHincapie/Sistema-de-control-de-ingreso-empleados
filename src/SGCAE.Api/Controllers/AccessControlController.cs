using MediatR;
using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Features.AccessControl.Commands.RegisterAccessMovement;
using SGCAE.Application.Features.AccessControl.DTOs;
using SGCAE.Application.Features.AccessControl.Queries.GetAccessHistory;
using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;

namespace SGCAE.Api.Controllers;

/// <summary>
/// Endpoints para el registro de entradas/salidas (RF-03) y consulta paginada del historial de accesos (RF-04).
/// </summary>
[ApiController]
[Route("api/v1/access-control")]
[Produces("application/json")]
public sealed class AccessControlController : ControllerBase
{
    private readonly ISender _sender;

    public AccessControlController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra una solicitud de acceso (entrada o salida), valida las condiciones del empleado,
    /// la credencial y el punto de acceso, y persiste el movimiento autorizado o rechazado (RF-03).
    /// </summary>
    [HttpPost("movements")]
    [ProducesResponseType(typeof(AccessLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterMovement(
        [FromBody] RegisterAccessMovementCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el historial de movimientos de acceso con filtros combinables y paginación obligatoria (RF-04).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<AccessLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? accessPointId,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] AccessValidationResult? validationResult,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccessHistoryQuery(
            employeeId,
            accessPointId,
            fromDateUtc,
            toDateUtc,
            validationResult,
            pageNumber,
            pageSize);

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
