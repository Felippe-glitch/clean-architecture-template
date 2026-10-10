using Template.Core.App.Auth.DataTransfer;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Auth.Service;

public interface IAuthAppService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Renews the access/refresh pair from a valid refresh token (stateless).</summary>
    Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
}
