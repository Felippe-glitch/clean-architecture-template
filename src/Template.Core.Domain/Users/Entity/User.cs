using Template.Core.Domain.Users.Enums;

namespace Template.Core.Domain.Users.Entity;

public class User
{
    public virtual int Id { get; protected set; }
    public virtual string Login { get; protected set; }
    public virtual string PasswordHash { get; protected set; }
    public virtual string Email { get; protected set; }
    public virtual UserRole Role { get; protected set; }
    public virtual bool Active { get; protected set; }

    protected User() { }

    public User(string login, string passwordHash, string email, UserRole role)
    {
        SetLogin(login);
        SetPasswordHash(passwordHash);
        SetEmail(email);
        SetRole(role);
        Active = true;
    }

    public static string NormalizeLogin(string login) => (login ?? string.Empty).Trim().ToLowerInvariant();

    public virtual void SetLogin(string login)
    {
        string normalized = NormalizeLogin(login);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Login cannot be empty.");

        Login = normalized;
    }

    public virtual void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Invalid password hash.");

        PasswordHash = passwordHash;
    }

    public virtual void SetEmail(string email)
    {
        string normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Email cannot be empty.");

        Email = normalized;
    }

    public virtual void SetRole(UserRole role) => Role = role;

    public virtual void Activate()
    {
        if (!Active) Active = true;
    }

    public virtual void Deactivate()
    {
        if (Active) Active = false;
    }

}
