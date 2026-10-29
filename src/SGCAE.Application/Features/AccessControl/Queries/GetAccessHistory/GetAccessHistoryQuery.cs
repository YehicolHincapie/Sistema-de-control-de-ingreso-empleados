using FluentValidation;
using SGCAE.Application.Common.Messaging;
using SGCAE.Application.Features.AccessControl.DTOs;
using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Features.AccessControl.Queries.GetAccessHistory;

public sealed record GetAccessHistoryQuery(
    Guid? EmployeeId = null,
    Guid? AccessPointId = null,
    DateTime? FromDateUtc = null,
    DateTime? ToDateUtc = null,
    AccessValidationResult? ValidationResult = null,
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResult<AccessLogDto>>;

public sealed class GetAccessHistoryQueryValidator : AbstractValidator<GetAccessHistoryQuery>
{
    public GetAccessHistoryQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100 registros.");

        RuleFor(x => x)
            .Must(x => !x.FromDateUtc.HasValue || !x.ToDateUtc.HasValue || x.FromDateUtc <= x.ToDateUtc)
            .WithMessage("La fecha inicial ('FromDateUtc') no puede ser posterior a la fecha final ('ToDateUtc').");
    }
}

public sealed class GetAccessHistoryQueryHandler : IQueryHandler<GetAccessHistoryQuery, PagedResult<AccessLogDto>>
{
    private readonly IAccessLogRepository _accessLogRepository;

    public GetAccessHistoryQueryHandler(IAccessLogRepository accessLogRepository)
    {
        _accessLogRepository = accessLogRepository;
    }

    public async Task<PagedResult<AccessLogDto>> Handle(GetAccessHistoryQuery request, CancellationToken cancellationToken)
    {
        var pagedLogs = await _accessLogRepository.GetPagedHistoryAsync(
            request.EmployeeId,
            request.AccessPointId,
            request.FromDateUtc,
            request.ToDateUtc,
            request.ValidationResult,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtoItems = pagedLogs.Items.Select(log => log.ToDto()).ToList();

        return new PagedResult<AccessLogDto>(
            dtoItems,
            pagedLogs.TotalCount,
            pagedLogs.PageNumber,
            pagedLogs.PageSize);
    }
}
