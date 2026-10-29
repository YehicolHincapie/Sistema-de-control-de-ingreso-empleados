using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Commands.RevokeCredential;

public sealed record RevokeCredentialCommand(
    string Code,
    string? Reason = null) : ICommand<CredentialDto>;

public sealed class RevokeCredentialCommandValidator : AbstractValidator<RevokeCredentialCommand>
{
    public RevokeCredentialCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de la credencial es obligatorio.");

        RuleFor(x => x.Reason)
            .MaximumLength(250).WithMessage("El motivo de revocación no puede exceder los 250 caracteres.");
    }
}

public sealed class RevokeCredentialCommandHandler : ICommandHandler<RevokeCredentialCommand, CredentialDto>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeCredentialCommandHandler(ICredentialRepository credentialRepository, IUnitOfWork unitOfWork)
    {
        _credentialRepository = credentialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CredentialDto> Handle(RevokeCredentialCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var credential = await _credentialRepository.GetByCodeAsync(normalizedCode, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la credencial con código '{normalizedCode}'.");

        credential.Revoke(request.Reason);

        _credentialRepository.Update(credential);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return credential.ToDto();
    }
}

