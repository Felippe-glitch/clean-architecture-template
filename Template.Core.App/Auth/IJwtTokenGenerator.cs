using Template.Core.Domain.Users.Entity;
using System;

namespace Template.Core.App.Auth;

public interface IJwtTokenGenerator
{
    /// <summary>Generates the (short-lived) access token and returns it along with its expiration instant (UTC).</summary>
    (string Token, DateTime ExpiresAt) Generate(User user);

    /// <summary>Generates the refresh token (a long-lived JWT with a refresh audience) and its expiration (UTC).</summary>
    (string Token, DateTime ExpiresAt) GenerateRefresh(User user);

    /// <summary>Validates a refresh token statelessly. Returns the user's Id if valid, or <c>null</c>.</summary>
    int? ValidateRefreshToken(string refreshToken);
}
