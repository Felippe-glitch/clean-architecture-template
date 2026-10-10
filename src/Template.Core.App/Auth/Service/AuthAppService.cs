using Mapster;

using Microsoft.Extensions.Caching.Memory;

using Template.Core.App.Auth.DataTransfer;
using Template.Core.App.Users.DataTransfer;
using Template.Core.CrossCutting.Security;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Interfaces.Service;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Template.Core.App.Auth.Service;

public class AuthAppService(
    IUserService userService,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IMemoryCache cache,
    LoginLockoutSettings lockout) : IAuthAppService
{
    /// <summary>
    /// Single message for wrong credentials, inactive account, nonexistent account, and lockout.
    /// If lockout had its own message, it would become an account-enumeration oracle.
    /// </summary>
    private const string InvalidCredentials = "Invalid login or password.";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        string key = LockoutCacheKey(request.Login);
        cache.TryGetValue(key, out int failures);

        // Before the lookup and BCrypt: also removes the hash's CPU-amplification as an attack vector.
        if (failures >= lockout.LockoutAttempts)
            throw new UnauthorizedAccessException(InvalidCredentials);

        User? user = await userService.GetByLogin(request.Login, cancellationToken);

        if (user is null || !user.Active || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            RecordFailure(key, failures);
            throw new UnauthorizedAccessException(InvalidCredentials);
        }

        cache.Remove(key);

        return IssueSession(user);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        int? userId = tokenGenerator.ValidateRefreshToken(refreshToken);
        if (userId is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        User user = await userService.Validate(userId.Value, cancellationToken);
        if (!user.Active)
            throw new UnauthorizedAccessException("Inactive user.");

        return IssueSession(user);
    }

    /// <summary>Keyed by the normalized login — the same criterion used for lookup, otherwise varying case would bypass the lockout.</summary>
    private static string LockoutCacheKey(string login) => $"lockout:login:{User.NormalizeLogin(login)}";

    /// <summary>
    /// Renews the window on every failure: the lockout lasts <c>LockoutMinutes</c> from the
    /// last attempt, so retrying only extends the lockout itself.
    /// </summary>
    private void RecordFailure(string key, int failures)
    {
        cache.Set(key, failures + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(lockout.LockoutMinutes),
        });
    }

    /// <summary>Issues a new access + refresh pair (both JWTs, stateless).</summary>
    private LoginResponse IssueSession(User user)
    {
        (string token, DateTime expiresAt) = tokenGenerator.Generate(user);
        (string refresh, DateTime refreshExpiresAt) = tokenGenerator.GenerateRefresh(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refresh,
            RefreshExpiresAt = refreshExpiresAt,
            User = user.Adapt<UserResponse>(),
        };
    }
}
