using System.Text;

namespace Template.Core.App.Auth;

/// <summary>Parâmetros de assinatura/validação do JWT (seção <c>Jwt</c> da configuração).</summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Template";
    public string Audience { get; set; } = "Template";
    /// <summary>Tempo de vida do access token, em minutos (curto; padrão: 15).</summary>
    public int ExpiraMinutos { get; set; } = 15;
    /// <summary>Tempo de vida do refresh token, em dias (padrão: 7).</summary>
    public int RefreshExpiraDias { get; set; } = 7;

    /// <summary>
    /// Audience do refresh token — distinta do access de propósito. O middleware JwtBearer
    /// valida <see cref="Audience"/>, então um refresh (com esta audience) é rejeitado nas
    /// rotas protegidas, impedindo usar o token longo como bearer.
    /// </summary>
    public string RefreshAudience => $"{Audience}:refresh";

    /// <summary>
    /// Mínimo de bytes da chave de assinatura. É o tamanho do bloco do HMAC-SHA256: uma
    /// chave menor não acrescenta entropia além do próprio comprimento e enfraquece a
    /// assinatura.
    /// </summary>
    public const int TamanhoMinimoChaveBytes = 32;

    public bool EstaConfigurado => !string.IsNullOrWhiteSpace(Key);

    /// <summary>Tamanho da chave em bytes UTF-8 — a mesma codificação usada para assinar.</summary>
    public int TamanhoChaveBytes => Encoding.UTF8.GetByteCount(Key ?? string.Empty);

    public bool ChaveTemForcaSuficiente => TamanhoChaveBytes >= TamanhoMinimoChaveBytes;
}
