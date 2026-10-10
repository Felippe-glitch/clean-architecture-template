using System.Text;

namespace Template.Core.App.Auth;

/// <summary>JWT signing/validation parameters (the <c>Jwt</c> configuration section).</summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Template";
    public string Audience { get; set; } = "Template";
    /// <summary>Access token lifetime, in minutes (short-lived; default: 15).</summary>
    public int ExpirationMinutes { get; set; } = 15;
    /// <summary>Refresh token lifetime, in days (default: 7).</summary>
    public int RefreshExpirationDays { get; set; } = 7;

    /// <summary>
    /// Audience of the refresh token — intentionally distinct from the access token's. The
    /// JwtBearer middleware validates <see cref="Audience"/>, so a refresh token (with this
    /// audience) is rejected on protected routes, preventing the long-lived token from being
    /// used as a bearer token.
    /// </summary>
    public string RefreshAudience => $"{Audience}:refresh";

    /// <summary>
    /// Minimum size of the signing key, in bytes. This is the HMAC-SHA256 block size: a
    /// smaller key adds no entropy beyond its own length and weakens the signature.
    /// </summary>
    public const int MinimumSigningKeyBytes = 32;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Key);

    /// <summary>Size of the key in UTF-8 bytes — the same encoding used to sign.</summary>
    public int SigningKeyBytes => Encoding.UTF8.GetByteCount(Key ?? string.Empty);

    public bool HasSufficientKeyStrength => SigningKeyBytes >= MinimumSigningKeyBytes;
}
