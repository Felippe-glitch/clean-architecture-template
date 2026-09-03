using Microsoft.AspNetCore.HttpOverrides;

namespace Template.Core.Api.Config;

/// <summary>
/// Trust in proxy headers (X-Forwarded-For/-Proto). Restores the https scheme and the
/// real client IP behind Render's edge, which the rate limiter depends on to partition
/// requests. Trusted networks live in <c>ForwardedHeaders:TrustedNetworks</c>.
/// Lives in the API project because it depends on the ASP.NET Core framework (HttpOverrides).
/// </summary>
public static class ForwardedHeadersConfig
{
    /// <summary>
    /// Private ranges (RFC 1918) plus loopback. On Render the edge reaches the container
    /// over the private network, so the peer always falls into one of these.
    /// </summary>
    private static readonly string[] DefaultNetworks =
    [
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "127.0.0.1/32",
    ];

    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        string[] cidrs = configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? DefaultNetworks;

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Only the last hop, which is Render's proxy. The proxy appends the real client IP
            // as the last entry of the XFF; with ForwardLimit = 1 the middleware consumes only
            // that entry, and any XFF forged by the client sits to the left and is ignored.
            options.ForwardLimit = 1;

            // Clears the framework defaults (::1/127.0.0.1) so the allowlist below is the
            // single source of truth. If the app is ever exposed directly, the peer is public,
            // no network matches, and the XFF is simply not trusted — a safe failure mode.
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (string cidr in cidrs)
            {
                // System.Net.IPNetwork, not Microsoft.AspNetCore.HttpOverrides.IPNetwork
                // (obsolete since .NET 8 and ambiguous with the using above).
                if (System.Net.IPNetwork.TryParse(cidr, out System.Net.IPNetwork network))
                    options.KnownIPNetworks.Add(network);
                else
                    throw new InvalidOperationException(
                        $"ForwardedHeaders:TrustedNetworks contains an invalid CIDR: '{cidr}'.");
            }
        });

        return services;
    }
}
