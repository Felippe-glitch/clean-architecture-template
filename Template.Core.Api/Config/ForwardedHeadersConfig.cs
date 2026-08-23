using Microsoft.AspNetCore.HttpOverrides;

namespace Template.Core.Api.Config;

/// <summary>
/// Confiança nos headers de proxy (X-Forwarded-For/-Proto). Restaura o esquema https e o
/// IP real do cliente atrás da borda do Render — do qual o rate limiter depende para
/// particionar. Redes confiáveis em <c>ForwardedHeaders:TrustedNetworks</c>.
/// Fica no projeto da API por depender do framework ASP.NET Core (HttpOverrides).
/// </summary>
public static class ForwardedHeadersConfig
{
    /// <summary>
    /// Faixas privadas (RFC 1918) mais o loopback. No Render a borda alcança o container
    /// pela rede privada, então o peer sempre cai numa dessas.
    /// </summary>
    private static readonly string[] RedesPadrao =
    [
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "127.0.0.1/32",
    ];

    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        string[] cidrs = configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? RedesPadrao;

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Só o último hop, que é o proxy do Render. O proxy anexa o IP real do cliente
            // como última entrada do XFF; com ForwardLimit = 1 o middleware consome apenas
            // essa entrada, e um XFF forjado pelo cliente fica à esquerda e é ignorado.
            options.ForwardLimit = 1;

            // Zera os padrões do framework (::1/127.0.0.1) para a allowlist abaixo ser a
            // única fonte de verdade. Se a app um dia for exposta direto, o peer é público,
            // nenhuma rede casa e o XFF simplesmente não é confiado — falha segura.
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (string cidr in cidrs)
            {
                // System.Net.IPNetwork, e não o Microsoft.AspNetCore.HttpOverrides.IPNetwork
                // (obsoleto desde o .NET 8 e ambíguo com o using acima).
                if (System.Net.IPNetwork.TryParse(cidr, out System.Net.IPNetwork rede))
                    options.KnownIPNetworks.Add(rede);
                else
                    throw new InvalidOperationException(
                        $"ForwardedHeaders:TrustedNetworks contém um CIDR inválido: '{cidr}'.");
            }
        });

        return services;
    }
}
