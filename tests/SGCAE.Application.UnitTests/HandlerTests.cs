using NSubstitute;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Features.AccessControl.Commands.RegisterAccessMovement;
using SGCAE.Application.Features.AccessControl.Queries.GetAccessHistory;
using SGCAE.Application.Features.Credentials.Commands.AssignCredential;
using SGCAE.Application.Features.Employees.Commands.CreateEmployee;
using SGCAE.Application.Features.Employees.Commands.DeactivateEmployee;
using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.UnitTests;

public class HandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IEmployeeRepository _employees = Substitute.For<IEmployeeRepository>();
    private readonly ICredentialRepository _credentials = Substitute.For<ICredentialRepository>();
    private readonly IAccessPointRepository _points = Substitute.For<IAccessPointRepository>();
    private readonly IAccessLogRepository _logs = Substitute.For<IAccessLogRepository>();

    private static Employee Emp() =>
        Employee.Create("1020304050", "Ana", "PÃ©rez", "ana@empresa.com", "Analista", "TI");

    // ---------- RF-01 ----------
    [Fact]
    public async Task CreateEmployee_Valid_PersistsAndReturnsDto()
    {
        var handler = new CreateEmployeeCommandHandler(_employees, _uow);
        var dto = await handler.Handle(
            new CreateEmployeeCommand("1020304050", "Ana", "PÃ©rez", "ana@empresa.com", "Analista", "TI"), default);

        Assert.Equal("1020304050", dto.IdentificationNumber);
        await _employees.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEmployee_DuplicateIdentification_ThrowsConflict()
    {
        _employees.ExistsByIdentificationAsync("1020304050", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateEmployeeCommandHandler(_employees, _uow);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new CreateEmployeeCommand("1020304050", "Ana", "PÃ©rez", "ana@empresa.com", "Analista", "TI"), default));
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEmployee_DuplicateEmail_ThrowsConflict()
    {
        _employees.ExistsByEmailAsync("ana@empresa.com", null, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateEmployeeCommandHandler(_employees, _uow);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new CreateEmployeeCommand("1020304050", "Ana", "PÃ©rez", "ana@empresa.com", "Analista", "TI"), default));
    }

    [Fact]
    public void CreateEmployeeValidator_RejectsEmptyFields()
    {
        var result = new CreateEmployeeCommandValidator()
            .Validate(new CreateEmployeeCommand("", "", "", "no-es-email", "", ""));

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }

    [Fact]
    public async Task DeactivateEmployee_RevokesNonRevokedCredentials()
    {
        var emp = Emp();
        var active = Credential.Issue("CRD-0001", emp);
        var blocked = Credential.Issue("CRD-0002", emp);
        blocked.Block();
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);
        _credentials.GetNonRevokedByEmployeeIdAsync(emp.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Credential> { active, blocked });

        var dto = await new DeactivateEmployeeCommandHandler(_employees, _credentials, _uow)
            .Handle(new DeactivateEmployeeCommand(emp.Id), default);

        Assert.Equal(EmploymentStatus.Inactive, dto.Status);
        Assert.Equal(CredentialStatus.Revoked, active.Status);
        Assert.Equal(CredentialStatus.Revoked, blocked.Status);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateEmployee_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new DeactivateEmployeeCommandHandler(_employees, _credentials, _uow)
                .Handle(new DeactivateEmployeeCommand(Guid.NewGuid()), default));
    }

    // ---------- RF-02 ----------
    [Fact]
    public async Task AssignCredential_EmployeeNotFound_ThrowsNotFound()
    {
        var handler = new AssignCredentialCommandHandler(_credentials, _employees, _uow);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new AssignCredentialCommand(Guid.NewGuid(), "CRD-0001"), default));
    }

    [Fact]
    public async Task AssignCredential_EmployeeAlreadyHasActive_ThrowsConflict()
    {
        var emp = Emp();
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);
        _credentials.GetActiveByEmployeeIdAsync(emp.Id, Arg.Any<CancellationToken>())
            .Returns(Credential.Issue("CRD-0001", emp));
        var handler = new AssignCredentialCommandHandler(_credentials, _employees, _uow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AssignCredentialCommand(emp.Id, "CRD-0002"), default));
    }

    [Fact]
    public async Task AssignCredential_DuplicateCode_ThrowsConflict()
    {
        var emp = Emp();
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);
        _credentials.ExistsByCodeAsync("CRD-0001", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new AssignCredentialCommandHandler(_credentials, _employees, _uow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AssignCredentialCommand(emp.Id, "CRD-0001"), default));
    }

    [Fact]
    public async Task AssignCredential_InactiveEmployee_ThrowsDomainException()
    {
        var emp = Emp();
        emp.Deactivate();
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);
        var handler = new AssignCredentialCommandHandler(_credentials, _employees, _uow);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new AssignCredentialCommand(emp.Id, "CRD-0001"), default));
    }

    [Fact]
    public async Task AssignCredential_Valid_Persists()
    {
        var emp = Emp();
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);
        var handler = new AssignCredentialCommandHandler(_credentials, _employees, _uow);

        var dto = await handler.Handle(new AssignCredentialCommand(emp.Id, "crd-0001"), default);

        Assert.Equal("CRD-0001", dto.Code);
        Assert.Equal(CredentialStatus.Active, dto.Status);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- RF-03 ----------
    private RegisterAccessMovementCommandHandler MovementHandler() =>
        new(_logs, _credentials, _employees, _points, _uow);

    [Fact]
    public async Task RegisterMovement_AccessPointNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => MovementHandler().Handle(
            new RegisterAccessMovementCommand("CRD-0001", Guid.NewGuid(), MovementType.Entry), default));
    }

    [Fact]
    public async Task RegisterMovement_ValidCredential_IsAuthorizedAndPersisted()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        var ap = AccessPoint.Create("AP-1", "RecepciÃ³n", "Piso 1");
        _points.GetByIdAsync(ap.Id, Arg.Any<CancellationToken>()).Returns(ap);
        _credentials.GetByCodeAsync("CRD-0001", Arg.Any<CancellationToken>()).Returns(cred);
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);

        var dto = await MovementHandler().Handle(
            new RegisterAccessMovementCommand("crd-0001", ap.Id, MovementType.Entry), default);

        Assert.True(dto.IsAuthorized);
        await _logs.Received(1).AddAsync(Arg.Any<AccessLog>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterMovement_UnknownCredential_IsRejectedButStillPersisted()
    {
        var ap = AccessPoint.Create("AP-1", "RecepciÃ³n", "Piso 1");
        _points.GetByIdAsync(ap.Id, Arg.Any<CancellationToken>()).Returns(ap);

        var dto = await MovementHandler().Handle(
            new RegisterAccessMovementCommand("NOEXISTE-1", ap.Id, MovementType.Exit), default);

        Assert.False(dto.IsAuthorized);
        Assert.Equal(AccessValidationResult.Rejected, dto.ValidationResult);
        await _logs.Received(1).AddAsync(Arg.Any<AccessLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterMovement_InactiveEmployee_IsRejected()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        emp.Deactivate();
        var ap = AccessPoint.Create("AP-1", "RecepciÃ³n", "Piso 1");
        _points.GetByIdAsync(ap.Id, Arg.Any<CancellationToken>()).Returns(ap);
        _credentials.GetByCodeAsync("CRD-0001", Arg.Any<CancellationToken>()).Returns(cred);
        _employees.GetByIdAsync(emp.Id, Arg.Any<CancellationToken>()).Returns(emp);

        var dto = await MovementHandler().Handle(
            new RegisterAccessMovementCommand("CRD-0001", ap.Id, MovementType.Entry), default);

        Assert.False(dto.IsAuthorized);
        Assert.Contains("laboral", dto.RejectionReason);
    }

    // ---------- RF-04 ----------
    [Fact]
    public void HistoryValidator_RejectsBadPaging_AndInvertedDates()
    {
        var v = new GetAccessHistoryQueryValidator();

        Assert.False(v.Validate(new GetAccessHistoryQuery(PageNumber: 0)).IsValid);
        Assert.False(v.Validate(new GetAccessHistoryQuery(PageSize: 101)).IsValid);
        Assert.False(v.Validate(new GetAccessHistoryQuery(
            FromDateUtc: DateTime.UtcNow, ToDateUtc: DateTime.UtcNow.AddDays(-1))).IsValid);
        Assert.True(v.Validate(new GetAccessHistoryQuery(PageNumber: 2, PageSize: 20)).IsValid);
    }

    [Fact]
    public async Task GetHistory_ReturnsPagedResult()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        var ap = AccessPoint.Create("AP-1", "RecepciÃ³n", "Piso 1");
        var log = AccessLog.EvaluateAccessAttempt("CRD-0001", cred, emp, ap, MovementType.Entry);
        _logs.GetPagedHistoryAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<AccessLog>([log], 25, 1, 10));

        var result = await new GetAccessHistoryQueryHandler(_logs).Handle(new GetAccessHistoryQuery(), default);

        Assert.Single(result.Items);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
    }
}

