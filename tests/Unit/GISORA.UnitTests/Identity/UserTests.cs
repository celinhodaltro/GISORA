using GISORA.Domain.Common;
using GISORA.Domain.Identity;

namespace GISORA.UnitTests.Identity;

public class UserTests
{
    private static readonly HashedPassword Password = new("hash", "salt");

    [Fact]
    public void CreateNormalizesEmailAndRaisesEvent()
    {
        var user = User.Create("  Maria Silva  ", "  Maria@GISORA.gov.br ", Password, UserRole.Analyst, DateTimeOffset.UtcNow);

        Assert.Equal("Maria Silva", user.Name);
        Assert.Equal("maria@gisora.gov.br", user.Email);
        Assert.True(user.IsActive);
        Assert.IsType<UserRegistered>(Assert.Single(user.DomainEvents));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsBlankName(string name)
    {
        Assert.Throws<DomainException>(() =>
            User.Create(name, "maria@gisora.gov.br", Password, UserRole.Analyst, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("@gisora.gov.br")]
    [InlineData("maria@")]
    [InlineData("maria silva@gisora.gov.br")]
    public void RejectsInvalidEmail(string email)
    {
        var exception = Assert.Throws<DomainException>(() =>
            User.Create("Maria", email, Password, UserRole.Analyst, DateTimeOffset.UtcNow));

        Assert.Equal("identity.user.email-invalid", exception.Code);
    }

    [Fact]
    public void RejectsEmptyPasswordHash()
    {
        Assert.Throws<DomainException>(() =>
            User.Create("Maria", "maria@gisora.gov.br", new HashedPassword("", "salt"), UserRole.Analyst, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AdministratorHasAllPermissions()
    {
        var user = User.Create("Admin", "admin@gisora.gov.br", Password, UserRole.Administrator, DateTimeOffset.UtcNow);

        Assert.True(user.HasPermission(AccessRight.ManageUsers));
        Assert.True(user.HasPermission(AccessRight.EditCadastral));
    }

    [Fact]
    public void PublicViewerOnlySeesOverview()
    {
        var user = User.Create("Consulta", "consulta@gisora.gov.br", Password, UserRole.PublicViewer, DateTimeOffset.UtcNow);

        Assert.True(user.HasPermission(AccessRight.ViewOverview));
        Assert.False(user.HasPermission(AccessRight.EditCadastral));
        Assert.False(user.HasPermission(AccessRight.ManageUsers));
    }

    [Fact]
    public void DeactivateAndActivateToggleState()
    {
        var user = User.Create("Maria", "maria@gisora.gov.br", Password, UserRole.Analyst, DateTimeOffset.UtcNow);

        user.Deactivate();
        Assert.False(user.IsActive);

        user.Activate();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void ChangeRoleRejectsUndefinedRole()
    {
        var user = User.Create("Maria", "maria@gisora.gov.br", Password, UserRole.Analyst, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => user.ChangeRole((UserRole)99));
    }

    [Fact]
    public void ReplacePasswordUpdatesSecret()
    {
        var user = User.Create("Maria", "maria@gisora.gov.br", Password, UserRole.Analyst, DateTimeOffset.UtcNow);

        user.ReplacePassword(new HashedPassword("novo-hash", "novo-salt"));

        Assert.Equal("novo-hash", user.PasswordHash);
        Assert.Equal("novo-salt", user.PasswordSalt);
    }

    [Fact]
    public void RecordSignInStoresInstant()
    {
        var user = User.Create("Maria", "maria@gisora.gov.br", Password, UserRole.Analyst, DateTimeOffset.UtcNow);
        var instant = DateTimeOffset.UtcNow;

        user.RecordSignIn(instant);

        Assert.Equal(instant, user.LastSignInAt);
    }
}