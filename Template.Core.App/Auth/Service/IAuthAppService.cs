using Template.Core.App.Auth.DataTransfer;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Core.App.Auth.Service;

public interface IAuthAppService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Renova o par access/refresh a partir de um refresh token válido (stateless).</summary>
    Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
}
