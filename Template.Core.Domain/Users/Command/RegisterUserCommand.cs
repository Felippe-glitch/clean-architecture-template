using Template.Core.Domain.Users.Enums;

namespace Template.Core.Domain.Users.Command;

/// <summary>
/// Command to register a user. The password already arrives <b>hashed</b>
/// (the application layer hashes it before invoking the domain).
/// </summary>
public record RegisterUserCommand
{
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
}
