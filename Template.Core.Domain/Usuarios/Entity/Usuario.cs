using Template.Core.Domain.Usuarios.Enums;

namespace Template.Core.Domain.Usuarios.Entity;

public class Usuario
{
    public virtual int Id { get; protected set; }
    public virtual string Login { get; protected set; }
    public virtual string SenhaHash { get; protected set; }
    public virtual string Email { get; protected set; }
    public virtual UsuarioRoleEnum Role { get; protected set; }
    public virtual bool Ativo { get; protected set; }

    protected Usuario() { }

    public Usuario(string login, string senhaHash, string email, UsuarioRoleEnum role)
    {
        SetLogin(login);
        SetSenhaHash(senhaHash);
        SetEmail(email);
        SetRole(role);
        Ativo = true;
    }

    public static string NormalizarLogin(string login) => (login ?? string.Empty).Trim().ToLowerInvariant();

    public virtual void SetLogin(string login)
    {
        string normalizado = NormalizarLogin(login);
        if (string.IsNullOrWhiteSpace(normalizado))
            throw new ArgumentException("Login não pode ser vazio.");

        Login = normalizado;
    }

    public virtual void SetSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Hash de senha inválido.");

        SenhaHash = senhaHash;
    }

    public virtual void SetEmail(string email)
    {
        string normalizado = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizado))
            throw new ArgumentException("E-mail não pode ser vazio.");

        Email = normalizado;
    }

    public virtual void SetRole(UsuarioRoleEnum role) => Role = role;

    public virtual void Ativar()
    {
        if (!Ativo) Ativo = true;
    }

    public virtual void Desativar()
    {
        if (Ativo) Ativo = false;
    }

}
