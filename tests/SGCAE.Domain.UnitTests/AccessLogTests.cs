using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.UnitTests;

public class AccessLogTests
{
    private static Employee Emp() =>
        Employee.Create("1020304050", "Ana", "Pérez", "ana@empresa.com", "Analista", "TI");

    private static AccessPoint Point(bool enabled = true) =>
        AccessPoint.Create("AP-01", "Recepción", "Piso 1", isEnabled: enabled);

    [Fact]
    public void Evaluate_AllValid_ShouldBeAuthorized()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);

        var log = AccessLog.EvaluateAccessAttempt("crd-0001", cred, emp, Point(), MovementType.Entry);

        Assert.Equal(AccessValidationResult.Authorized, log.ValidationResult);
        Assert.Null(log.RejectionReason);
        Assert.Equal(MovementType.Entry, log.MovementType);
        Assert.Equal("CRD-0001", log.CredentialCodeUsed);
    }

    [Fact]
    public void Evaluate_UnknownCredential_ShouldBeRejected()
    {
        var log = AccessLog.EvaluateAccessAttempt("XXXX-9999", null, null, Point(), MovementType.Entry);

        Assert.Equal(AccessValidationResult.Rejected, log.ValidationResult);
        Assert.Contains("no registrada", log.RejectionReason);
        Assert.Null(log.CredentialId);
    }

    [Fact]
    public void Evaluate_BlockedCredential_ShouldBeRejected()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        cred.Block();

        var log = AccessLog.EvaluateAccessAttempt("CRD-0001", cred, emp, Point(), MovementType.Exit);

        Assert.Equal(AccessValidationResult.Rejected, log.ValidationResult);
        Assert.Contains("bloqueada", log.RejectionReason);
    }

    [Fact]
    public void Evaluate_RevokedCredential_ShouldBeRejected()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        cred.Revoke();

        var log = AccessLog.EvaluateAccessAttempt("CRD-0001", cred, emp, Point(), MovementType.Entry);

        Assert.Equal(AccessValidationResult.Rejected, log.ValidationResult);
        Assert.Contains("revocada", log.RejectionReason);
    }

    [Fact]
    public void Evaluate_InactiveEmployee_ShouldBeRejected()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);
        emp.Deactivate();

        var log = AccessLog.EvaluateAccessAttempt("CRD-0001", cred, emp, Point(), MovementType.Entry);

        Assert.Equal(AccessValidationResult.Rejected, log.ValidationResult);
        Assert.Contains("laboral", log.RejectionReason);
        Assert.Equal(emp.Id, log.EmployeeId);
    }

    [Fact]
    public void Evaluate_DisabledAccessPoint_ShouldBeRejected()
    {
        var emp = Emp();
        var cred = Credential.Issue("CRD-0001", emp);

        var log = AccessLog.EvaluateAccessAttempt("CRD-0001", cred, emp, Point(enabled: false), MovementType.Entry);

        Assert.Equal(AccessValidationResult.Rejected, log.ValidationResult);
        Assert.Contains("inhabilitado", log.RejectionReason);
    }
}

