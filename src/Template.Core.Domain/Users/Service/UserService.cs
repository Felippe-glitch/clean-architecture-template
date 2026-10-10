using Template.Core.CrossCutting.Exceptions;
using Template.Core.Domain.Users.Command;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Enums;
using Template.Core.Domain.Users.Interfaces.Service;
using Template.Core.Domain.Users.Repository;

namespace Template.Core.Domain.Users.Service;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User> Register(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {

        string login = User.NormalizeLogin(command.Login);

        User existing = await userRepository.GetByLoginAsync(login, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException("A user with this login already exists.");

        User user = new(command.Login, command.PasswordHash, command.Email, command.Role);
        await userRepository.InsertAsync(user, cancellationToken);

        return user;
    }

    public async Task<User?> GetByLogin(string login, CancellationToken cancellationToken = default)
        => await userRepository.GetByLoginAsync(User.NormalizeLogin(login), cancellationToken);

    public async Task<User> Validate(int id, CancellationToken cancellationToken = default)
        => await userRepository.GetAsync(id, cancellationToken)
           ?? throw new EntityNotFoundException(nameof(User));

    public async Task<User> ChangeStatus(
        int id,
        bool active,
        int actorId,
        CancellationToken cancellationToken = default)
    {
        if (id == actorId)
            throw new BusinessRuleException("You cannot change your own status.");

        User user = await Validate(id, cancellationToken);

        if (active)
            user.Activate();
        else
            user.Deactivate();

        await userRepository.UpdateAsync(user, cancellationToken);
        return user;
    }

    public async Task<bool> HasAny(CancellationToken cancellationToken = default)
        => await userRepository.HasAnyAsync(cancellationToken);
}
