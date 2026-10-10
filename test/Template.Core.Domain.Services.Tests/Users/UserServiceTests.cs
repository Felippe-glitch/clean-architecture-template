using NSubstitute;

using Template.Core.CrossCutting.Exceptions;
using Template.Core.Domain.Users.Command;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Enums;
using Template.Core.Domain.Users.Repository;
using Template.Core.Domain.Users.Service;

namespace Template.Core.Domain.Services.Tests.Users;

public class UserServiceTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly UserService sut;

    public UserServiceTests()
    {
        // User has only virtual members, so NSubstitute would auto-return a fake User
        // instead of null — make "not found" the explicit default.
        userRepository.GetByLoginAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User)null!);
        userRepository.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((User)null!);

        sut = new UserService(userRepository);
    }

    private static RegisterUserCommand CreateCommand() => new()
    {
        Login = " NewUser ",
        PasswordHash = "hash",
        Email = "new@mail.com",
        Role = UserRole.ADMIN,
    };

    [Fact]
    public async Task Register_WhenLoginIsFree_ShouldInsertUser()
    {
        User user = await sut.Register(CreateCommand(), TestContext.Current.CancellationToken);

        Assert.Equal("newuser", user.Login);
        await userRepository.Received(1).InsertAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WhenLoginAlreadyExists_ShouldThrow()
    {
        userRepository.GetByLoginAsync("newuser", Arg.Any<CancellationToken>())
            .Returns(new User("newuser", "hash", "other@mail.com", UserRole.ADMIN));

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.Register(CreateCommand(), TestContext.Current.CancellationToken));

        await userRepository.DidNotReceive().InsertAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_WhenUserDoesNotExist_ShouldThrow()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => sut.Validate(42, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ChangeStatus_WhenActorIsTheSameUser_ShouldThrow()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.ChangeStatus(1, active: false, actorId: 1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ChangeStatus_ShouldDeactivateAndPersist()
    {
        User user = new("target", "hash", "target@mail.com", UserRole.ADMIN);
        userRepository.GetAsync(2, Arg.Any<CancellationToken>()).Returns(user);

        User result = await sut.ChangeStatus(2, active: false, actorId: 1, TestContext.Current.CancellationToken);

        Assert.False(result.Active);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }
}
