using Template.Core.App.Usuarios.DataTransfer;
using System;

namespace Template.Core.App.Auth.DataTransfer;

public record LoginResponse
{
    public string Token { get; init; }
    public DateTime ExpiraEm { get; init; }
    public string RefreshToken { get; init; }
    public DateTime RefreshExpiraEm { get; init; }
    public UsuarioResponse Usuario { get; init; }
}
