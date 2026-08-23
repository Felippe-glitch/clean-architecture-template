namespace Template.Core.App.Auth;

/// <summary>
/// Lockout de login por conta (seção <c>RateLimit:Auth</c> da configuração). Defesa em
/// profundidade sobre o rate limit por IP: aquele depende da topologia do proxy, que pode
/// mudar sem aviso; este não depende de IP nenhum.
/// </summary>
public class LoginLockoutSettings
{
    /// <summary>Falhas consecutivas na mesma conta antes de bloquear (padrão: 5).</summary>
    public int LockoutFalhas { get; set; } = 5;

    /// <summary>Duração do bloqueio, contada a partir da última falha (padrão: 15 min).</summary>
    public int LockoutMinutos { get; set; } = 15;
}
