using GISORA.Application.Integration;
using GISORA.Infrastructure.Integrations.Cib;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GISORA.Infrastructure.Integrations;

public static class CibIntegrationRegistration
{
    public static IServiceCollection AddCibIntegration(this IServiceCollection services, IConfiguration configuration)
        => services.AddCibIntegration(
            configuration["Cib:Mode"] ?? "Demo",
            configuration["Cib:ProxyBaseUrl"]);

    public static IServiceCollection AddCibIntegration(
        this IServiceCollection services,
        string mode,
        string? proxyBaseUrl = null)
    {
        if (string.Equals(mode, "Production", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(proxyBaseUrl, UriKind.Absolute, out var baseAddress))
            {
                services.AddScoped<ICibPropertyGateway>(_ => new ProxyCibPropertyGateway(new HttpClient
                {
                    BaseAddress = baseAddress,
                    Timeout = TimeSpan.FromSeconds(15),
                }));
            }
            else
            {
                services.AddSingleton<ICibPropertyGateway>(new UnavailableCibPropertyGateway(
                    "Integracao SINTER em modo producao sem Cib:ProxyBaseUrl configurado."));
            }
        }
        else
        {
            services.AddSingleton<ICibPropertyGateway, DemoCibPropertyGateway>();
        }

        return services;
    }
}