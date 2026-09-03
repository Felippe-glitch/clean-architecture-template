namespace Template.Core.App.Auth;

/// <summary>
/// Per-account login lockout (the <c>RateLimit:Auth</c> configuration section). Defense in
/// depth on top of the per-IP rate limit: the latter depends on the proxy topology, which
/// can change without notice; this one doesn't depend on any IP.
/// </summary>
public class LoginLockoutSettings
{
    /// <summary>Consecutive failures on the same account before locking it out (default: 5).</summary>
    public int LockoutAttempts { get; set; } = 5;

    /// <summary>Lockout duration, counted from the last failure (default: 15 min).</summary>
    public int LockoutMinutes { get; set; } = 15;
}
