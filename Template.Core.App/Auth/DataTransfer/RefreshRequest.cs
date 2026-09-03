using System.ComponentModel.DataAnnotations;

namespace Template.Core.App.Auth.DataTransfer;

/// <summary>Body used to renew (refresh) the session. There is no logout — see ADR 08 and finding F4.</summary>
public record RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; }
}
