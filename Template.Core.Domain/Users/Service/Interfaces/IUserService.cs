using Template.Core.Domain.Users.Command;
using Template.Core.Domain.Users.Entity;

namespace Template.Core.Domain.Users.Interfaces.Service;

public interface IUserService
{
    Task<User> Register(RegisterUserCommand command, CancellationToken cancellationToken = default);
    Task<User?> GetByLogin(string login, CancellationToken cancellationToken = default);
    Task<User> Validate(int id, CancellationToken cancellationToken = default);
    Task<User> ChangeStatus(int id, bool active, int actorId, CancellationToken cancellationToken = default);
    Task<bool> HasAny(CancellationToken cancellationToken = default);
}
