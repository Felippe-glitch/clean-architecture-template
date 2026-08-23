using System.ComponentModel.DataAnnotations;

namespace Template.Core.App.Auth.DataTransfer;

/// <summary>Corpo para renovar (refresh) a sessão. Não existe logout — ver ADR 08 e o achado F4.</summary>
public record RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; }
}
