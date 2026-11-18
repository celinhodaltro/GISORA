using GISORA.Application.Identity.Commands;
using GISORA.Infrastructure;
using GISORA.Infrastructure.Integrations;
using GISORA.Infrastructure.Persistence;
using GISORA.Presentation;
using GISORA.Presentation.Shared.Layout;
using GISORA.Presentation.Shared.Security;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddGisoraCore();
builder.Services.AddCibIntegration(builder.Configuration);
builder.Services.AddGisoraSecurity();
builder.Services.AddScoped<MunicipalityState>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<StorageSchemaMigrator>().MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<EnsureIdentityUsersHandler>().HandleAsync();
}

await host.RunAsync();