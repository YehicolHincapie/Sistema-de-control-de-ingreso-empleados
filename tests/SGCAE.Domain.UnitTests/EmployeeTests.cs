using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.UnitTests;

public class EmployeeTests
{
    private static Employee NewEmployee() =>
        Employee.Create("1020304050", "Ana", "Pérez", "ana@empresa.com", "Analista", "TI");

    [Fact]
    public void Create_WithValidData_ShouldBeActiveAndNormalized()
    {
        var e = Employee.Create(" 1020abc ", " Ana ", " Pérez ", " ANA@Empresa.COM ", "Analista", "TI");

        Assert.Equal(EmploymentStatus.Active, e.Status);
        Assert.Equal("1020ABC", e.IdentificationNumber.Value);
        Assert.Equal("ana@empresa.com", e.Email.Value);
        Assert.Equal("Ana Pérez", e.FullName);
        Assert.True(e.IsActive());
    }

    [Theory]
    [InlineData("", "Ana", "Pérez", "a@b.com")]
    [InlineData("123456", "", "Pérez", "a@b.com")]
    [InlineData("123456", "Ana", "", "a@b.com")]
    [InlineData("123456", "Ana", "Pérez", "correo-invalido")]
    [InlineData("12", "Ana", "Pérez", "a@b.com")]
    [InlineData("12 34$", "Ana", "Pérez", "a@b.com")]
    public void Create_WithInvalidData_ShouldThrowDomainException(string id, string fn, string ln, string email)
    {
        Assert.Throws<DomainException>(() => Employee.Create(id, fn, ln, email, "Cargo", "Area"));
    }

    [Fact]
    public void Deactivate_ShouldSetInactive_AndFailIfAlreadyInactive()
    {
        var e = NewEmployee();
        e.Deactivate();

        Assert.Equal(EmploymentStatus.Inactive, e.Status);
        Assert.NotNull(e.DeactivatedAtUtc);
        Assert.False(e.IsActive());
        Assert.Throws<DomainException>(() => e.Deactivate());
    }

    [Fact]
    public void Update_ShouldChangeFields()
    {
        var e = NewEmployee();
        e.Update("Luis", "Gómez", "luis@empresa.com", "Jefe", "Ventas");

        Assert.Equal("Luis Gómez", e.FullName);
        Assert.Equal("luis@empresa.com", e.Email.Value);
        Assert.Equal("Ventas", e.Department);
        Assert.NotNull(e.UpdatedAtUtc);
    }
}

