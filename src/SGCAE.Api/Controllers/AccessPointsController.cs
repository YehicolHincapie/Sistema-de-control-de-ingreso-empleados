using MediatR;
using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Features.AccessPoints.Commands.ChangeAccessPointStatus;
using SGCAE.Application.Features.AccessPoints.Commands.CreateAccessPoint;
using SGCAE.Application.Features.AccessPoints.DTOs;
using SGCAE.Application.Features.AccessPoints.Queries.GetAccessPoints;

namespace SGCAE.Api.Controllers;

/// <summary>
/// Endpoints para la administración de puntos de acceso físicos (RF-06).
/// </summary>
[ApiController]
[Route("api/v1/access-points")]
[Produces("application/json")]
public sealed class AccessPointsController : ControllerBase
{
    private readonly ISender _sender;

    public AccessPointsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Consulta los puntos de acceso físicos de la organización, con filtro opcional por estado habilitado (RF-06).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccessPointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isEnabled,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAccessPointsQuery(isEnabled), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo punto de acceso físico (recepción, parqueadero, bodega, área restringida) (RF-06).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccessPointCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Habilita o deshabilita un punto de acceso físico (RF-06).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeAccessPointStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ChangeAccessPointStatusCommand(id, request.IsEnabled), cancellationToken);
        return Ok(result);
    }
}

public sealed record ChangeAccessPointStatusRequest(bool IsEnabled);
