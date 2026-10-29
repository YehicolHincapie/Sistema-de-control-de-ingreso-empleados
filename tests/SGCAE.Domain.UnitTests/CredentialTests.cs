using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.UnitTests;

public class CredentialTests
{
    private static Employee ActiveEmployee() =>
        Employee.Create("1020304050", "Ana", "Pérez", "ana@empresa.com", "Analista", "TI");

    [Fact]
    public void Issue_ForActiveEmployee_ShouldBeActive()
    {
        var emp = ActiveEmployee();
        var c = Credential.Issue("crd-0001", emp);

        Assert.Equal("CRD-0001", c.Code.Value);
        Assert.Equal(CredentialStatus.Active, c.Status);
        Assert.Equal(emp.Id, c.EmployeeId);
        Assert.True(c.IsUsable());
    }

    [Fact]
    public void Issue_ForInactiveEmployee_ShouldThrow()
    {
        var emp = ActiveEmployee();
        emp.Deactivate();

        Assert.Throws<DomainException>(() => Credential.Issue("CRD-0001", emp));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("COD IGO$")]
    public void Issue_WithInvalidCode_ShouldThrow(string code)
    {
        Assert.Throws<DomainException>(() => Credential.Issue(code, ActiveEmployee()));
    }

    [Fact]
    public void Block_Then_Unblock_ShouldToggleStatus()
    {
        var c = Credential.Issue("CRD-0001", ActiveEmployee());

        c.Block("extravío");
        Assert.Equal(CredentialStatus.Blocked, c.Status);
        Assert.Equal("extravío", c.BlockReason);
        Assert.False(c.IsUsable());
        Assert.Throws<DomainException>(() => c.Block());

        c.Unblock();
        Assert.Equal(CredentialStatus.Active, c.Status);
        Assert.Null(c.BlockReason);
        Assert.Throws<DomainException>(() => c.Unblock());
    }

    [Fact]
    public void Revoke_IsDefinitive_CannotBlockOrUnblock()
    {
        var c = Credential.Issue("CRD-0001", ActiveEmployee());
        c.Revoke("fin de contrato");

        Assert.Equal(CredentialStatus.Revoked, c.Status);
        Assert.Throws<DomainException>(() => c.Unblock());
        Assert.Throws<DomainException>(() => c.Block());
        Assert.Throws<DomainException>(() => c.Revoke());
    }
}
