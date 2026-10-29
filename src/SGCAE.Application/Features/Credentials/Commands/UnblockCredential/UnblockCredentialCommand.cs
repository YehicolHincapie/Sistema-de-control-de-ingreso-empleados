using FluentValidation;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Commands.UnblockCredential;

public sealed record UnblockCredentialCommand(string Code) : ICommand<CredentialDto>;

public sealed class UnblockCredentialCommandValidator : AbstractValidator<UnblockCredentialCommand>
{
    public UnblockCredentialCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de la credencial es obligatorio.");
    }
}

public sealed class UnblockCredentialCommandHandler : ICommandHandler<UnblockCredentialCommand, CredentialDto>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnblockCredentialCommandHandler(ICredentialRepository credentialRepository, IUnitOfWork unitOfWork)
    {
        _credentialRepository = credentialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CredentialDto> Handle(UnblockCredentialCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var credential = await _credentialRepository.GetByCodeAsync(normalizedCode, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la credencial con código '{normalizedCode}'.");

        credential.Unblock();

        _credentialRepository.Update(credential);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return credential.ToDto();
    }
}

