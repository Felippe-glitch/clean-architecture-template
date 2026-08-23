using System.ComponentModel.DataAnnotations;

namespace Template.Core.App.Auth.DataTransfer;

public record LoginRequest
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string Senha { get; set; }
}
