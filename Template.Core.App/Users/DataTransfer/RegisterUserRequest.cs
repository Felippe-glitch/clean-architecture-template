using System.ComponentModel.DataAnnotations;

using Template.Core.Domain.Users.Enums;

namespace Template.Core.App.Users.DataTransfer;

public record RegisterUserRequest
{
    [Required]
    public string Login { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email.")]
    public string Email { get; set; }

    /// <summary>
    /// The user's role. Nullable on purpose: on a non-nullable enum <c>[Required]</c> is inert
    /// (the default value already satisfies validation), and omitting the field would silently
    /// create an <see cref="UserRole.ADMIN"/>, which is member 0 of the enum — finding F7 from
    /// the audit report.
    /// </summary>
    [Required(ErrorMessage = "The user's role is required.")]
    public UserRole? Role { get; set; }
}
