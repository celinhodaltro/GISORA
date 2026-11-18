using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace GISORA.Presentation.Shared.Security;

public static class SecurityServiceRegistration
{
    public static IServiceCollection AddGisoraSecurity(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddScoped<GisoraAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(
            provider => provider.GetRequiredService<GisoraAuthenticationStateProvider>());

        return services;
    }
}