using SGCAE.Domain.Common;
using SGCAE.Domain.Entities;

namespace SGCAE.Domain.UnitTests;

public class AccessPointTests
{
    [Fact]
    public void Create_ShouldNormalizeCode_AndBeEnabledByDefault()
    {
        var ap = AccessPoint.Create(" ap-bod-01 ", "Bodega", "Zona Norte", isRestrictedArea: true);

        Assert.Equal("AP-BOD-01", ap.Code);
        Assert.True(ap.IsEnabled);
        Assert.True(ap.IsRestrictedArea);
    }

    [Theory]
    [InlineData("", "N", "A")]
    [InlineData("AP-1", "", "A")]
    [InlineData("AP-1", "N", "")]
    [InlineData("X", "N", "A")]
    public void Create_Invalid_ShouldThrow(string code, string name, string area)
    {
        Assert.Throws<DomainException>(() => AccessPoint.Create(code, name, area));
    }

    [Fact]
    public void Disable_Then_Enable_ShouldToggle_AndRejectRedundantChanges()
    {
        var ap = AccessPoint.Create("AP-1", "Recepción", "Piso 1");

        Assert.Throws<DomainException>(() => ap.Enable());
        ap.Disable();
        Assert.False(ap.IsEnabled);
        Assert.Throws<DomainException>(() => ap.Disable());
        ap.Enable();
        Assert.True(ap.IsEnabled);
    }
}

