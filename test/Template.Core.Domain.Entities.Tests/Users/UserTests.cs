using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Enums;

namespace Template.Core.Domain.Entities.Tests.Users;

public class UserTests
{
    private static User CreateUser() => new("  Admin ", "hash", " Admin@Mail.com ", UserRole.ADMIN);

    [Fact]
    public void Constructor_ShouldNormalizeLoginAndEmail_AndStartActive()
    {
        User user = CreateUser();

        Assert.Equal("admin", user.Login);
        Assert.Equal("admin@mail.com", user.Email);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(UserRole.ADMIN, user.Role);
        Assert.True(user.Active);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLogin_WhenEmpty_ShouldThrow(string? login)
    {
        User user = CreateUser();

        Assert.Throws<ArgumentException>(() => user.SetLogin(login!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetEmail_WhenEmpty_ShouldThrow(string? email)
    {
        User user = CreateUser();

        Assert.Throws<ArgumentException>(() => user.SetEmail(email!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPasswordHash_WhenEmpty_ShouldThrow(string? passwordHash)
    {
        User user = CreateUser();

        Assert.Throws<ArgumentException>(() => user.SetPasswordHash(passwordHash!));
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldToggleActive()
    {
        User user = CreateUser();

        user.Deactivate();
        Assert.False(user.Active);

        user.Activate();
        Assert.True(user.Active);
    }
}
