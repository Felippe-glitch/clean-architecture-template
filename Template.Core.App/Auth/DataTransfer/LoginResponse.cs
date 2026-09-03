using Template.Core.App.Users.DataTransfer;
using System;

namespace Template.Core.App.Auth.DataTransfer;

public record LoginResponse
{
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string RefreshToken { get; init; }
    public DateTime RefreshExpiresAt { get; init; }
    public UserResponse User { get; init; }
}
