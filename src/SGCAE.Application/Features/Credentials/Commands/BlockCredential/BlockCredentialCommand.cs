using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Commands.BlockCredential;

public sealed record BlockCredentialCommand(
    string Code,
    string? Reason = null) : ICommand<CredentialDto>;

public sealed class BlockCredentialCommandValidator : AbstractValidator<BlockCredentialCommand>
{
    public BlockCredentialCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de la credencial es obligatorio.");

        RuleFor(x => x.Reason)
            .MaximumLength(250).WithMessage("El motivo de bloqueo no puede exceder los 250 caracteres.");
    }
}

public sealed class BlockCredentialCommandHandler : ICommandHandler<BlockCredentialCommand, CredentialDto>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlockCredentialCommandHandler(ICredentialRepository credentialRepository, IUnitOfWork unitOfWork)
    {
        _credentialRepository = credentialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CredentialDto> Handle(BlockCredentialCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var credential = await _credentialRepository.GetByCodeAsync(normalizedCode, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la credencial con código '{normalizedCode}'.");

        credential.Block(request.Reason);

        _credentialRepository.Update(credential);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return credential.ToDto();
    }
}

