using Template.Core.App.Users.DataTransfer;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Users.Interfaces.Service;

public interface IUserAppService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> ChangeStatusAsync(int id, bool active, int actorId, CancellationToken cancellationToken = default);
    Task<ListUsersResponse> ListAsync(ListUsersRequest request, CancellationToken cancellationToken);
    
}
