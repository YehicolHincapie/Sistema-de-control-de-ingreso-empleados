using MediatR;
using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Features.Credentials.Commands.AssignCredential;
using SGCAE.Application.Features.Credentials.Commands.BlockCredential;
using SGCAE.Application.Features.Credentials.Commands.RevokeCredential;
using SGCAE.Application.Features.Credentials.Commands.UnblockCredential;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Application.Features.Credentials.Queries.GetCredentialByCode;

namespace SGCAE.Api.Controllers;

/// <summary>
/// Endpoints para la administración y control de credenciales de acceso (RF-02).
/// </summary>
[ApiController]
[Route("api/v1/credentials")]
[Produces("application/json")]
public sealed class CredentialsController : ControllerBase
{
    private readonly ISender _sender;

    public CredentialsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Consulta el estado y titular de una credencial por su código único (RF-02).
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCredentialByCodeQuery(code), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Asigna una nueva credencial de acceso a un empleado activo (RF-02).
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignCredentialCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
    }

    /// <summary>
    /// Bloquea temporalmente una credencial vigente (RF-02).
    /// </summary>
    [HttpPatch("{code}/block")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Block(
        string code,
        [FromBody] CredentialActionReasonRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new BlockCredentialCommand(code, request?.Reason), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Desbloquea una credencial previamente bloqueada (RF-02).
    /// </summary>
    [HttpPatch("{code}/unblock")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Unblock(string code, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnblockCredentialCommand(code), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revoca definitivamente una credencial impidiendo cualquier uso futuro (RF-02).
    /// </summary>
    [HttpPatch("{code}/revoke")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Revoke(
        string code,
        [FromBody] CredentialActionReasonRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RevokeCredentialCommand(code, request?.Reason), cancellationToken);
        return Ok(result);
    }
}

public sealed record CredentialActionReasonRequest(string? Reason);

