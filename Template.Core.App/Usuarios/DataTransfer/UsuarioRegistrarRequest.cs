using System.ComponentModel.DataAnnotations;

using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.App.Usuarios.DataTransfer;

public record UsuarioRegistrarRequest
{
    [Required]
    public string Login { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    public string Senha { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; }

    /// <summary>
    /// Papel do usuário. Nullable de propósito: em enum não-nullable o <c>[Required]</c> é inócuo
    /// (o valor default já satisfaz a validação) e omitir o campo criava silenciosamente um
    /// <see cref="UsuarioRoleEnum.ADMIN"/>, que é o membro 0 do enum — achado F7 do laudo.
    /// </summary>
    [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
    public UsuarioRoleEnum? Role { get; set; }
}
