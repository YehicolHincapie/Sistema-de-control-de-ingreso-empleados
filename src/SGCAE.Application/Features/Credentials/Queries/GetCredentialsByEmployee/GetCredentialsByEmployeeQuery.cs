using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.Credentials.DTOs;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.Credentials.Queries.GetCredentialsByEmployee;

public sealed record GetCredentialsByEmployeeQuery(Guid EmployeeId) : IQuery<IReadOnlyList<CredentialDto>>;

public sealed class GetCredentialsByEmployeeQueryHandler : IQueryHandler<GetCredentialsByEmployeeQuery, IReadOnlyList<CredentialDto>>
{
    private readonly ICredentialRepository _credentialRepository;

    public GetCredentialsByEmployeeQueryHandler(ICredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public async Task<IReadOnlyList<CredentialDto>> Handle(GetCredentialsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var credentials = await _credentialRepository.GetByEmployeeIdAsync(request.EmployeeId, cancellationToken);
        return credentials.Select(c => c.ToDto()).ToList();
    }
}

