using GISORA.Infrastructure.Integration.Cib;
using GISORA.Integration.Application;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GISORA.Infrastructure.Integration;

public static class CibIntegrationRegistration
{
    public static IServiceCollection AddCibIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var mode = configuration["Cib:Mode"] ?? "Demo";
        var proxyBaseUrl = configuration["Cib:ProxyBaseUrl"];

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