using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Queries.GetCredentialByCode;

public sealed record GetCredentialByCodeQuery(string Code) : IQuery<CredentialDto>;

public sealed class GetCredentialByCodeQueryHandler : IQueryHandler<GetCredentialByCodeQuery, CredentialDto>
{
    private readonly ICredentialRepository _credentialRepository;

    public GetCredentialByCodeQueryHandler(ICredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public async Task<CredentialDto> Handle(GetCredentialByCodeQuery request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var credential = await _credentialRepository.GetByCodeAsync(normalizedCode, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la credencial con código '{normalizedCode}'.");

        return credential.ToDto();
    }
}
