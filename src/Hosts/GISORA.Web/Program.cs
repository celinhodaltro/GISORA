using GISORA.Geospatial.Application;
using GISORA.Infrastructure.Geospatial;
using GISORA.Infrastructure.LocalStorage;
using GISORA.Infrastructure.LocalStorage.Repositories;
using GISORA.Properties.Application;
using GISORA.Properties.Application.Commands;
using GISORA.Properties.Application.Queries;
using GISORA.Properties.Domain;
using GISORA.PublicFacilities.Application;
using GISORA.PublicFacilities.Application.Commands;
using GISORA.PublicFacilities.Domain;
using GISORA.Roads.Application;
using GISORA.Roads.Application.Commands;
using GISORA.Roads.Domain;
using GISORA.Territory.Application;
using GISORA.Territory.Application.Commands;
using GISORA.Territory.Application.Queries;
using GISORA.Territory.Domain;
using GISORA.Web;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<ILocalStore, BrowserLocalStore>();
builder.Services.AddScoped<StorageSchemaMigrator>();

builder.Services.AddScoped<IMunicipalityRepository, LocalStorageMunicipalityRepository>();
builder.Services.AddScoped<IDistrictRepository, LocalStorageDistrictRepository>();
builder.Services.AddScoped<INeighborhoodRepository, LocalStorageNeighborhoodRepository>();
builder.Services.AddScoped<IFiscalSectorRepository, LocalStorageFiscalSectorRepository>();
builder.Services.AddScoped<IUrbanBlockRepository, LocalStorageUrbanBlockRepository>();
builder.Services.AddScoped<IUrbanBlockQueries, LocalStorageUrbanBlockQueries>();
builder.Services.AddScoped<ITerritoryQueries, LocalStorageTerritoryQueries>();

builder.Services.AddScoped<IPropertyRepository, LocalStoragePropertyRepository>();
builder.Services.AddScoped<ITaxpayerRepository, LocalStorageTaxpayerRepository>();
builder.Services.AddScoped<IPropertyQueries, LocalStoragePropertyQueries>();
builder.Services.AddScoped<IStreetRepository, LocalStorageStreetRepository>();
builder.Services.AddScoped<IStreetQueries, LocalStorageStreetQueries>();
builder.Services.AddScoped<CreateStreetHandler>();
builder.Services.AddScoped<IPublicFacilityRepository, LocalStorageFacilityRepository>();
builder.Services.AddScoped<IFacilityQueries, LocalStorageFacilityQueries>();
builder.Services.AddScoped<CreateFacilityHandler>();
builder.Services.AddScoped<IPropertyMapQueries, LocalStoragePropertyMapQueries>();
builder.Services.AddSingleton<IGeoJsonSerializer, GeoJsonSerializer>();

builder.Services.AddScoped<GeospatialInterop>();
builder.Services.AddScoped<IGeometryValidator, TurfGeometryValidator>();
builder.Services.AddScoped<ICoordinateTransformer, Proj4CoordinateTransformer>();
builder.Services.AddScoped<ISpatialOperations, TurfSpatialOperations>();

builder.Services.AddScoped<CreateMunicipalityHandler>();
builder.Services.AddScoped<CreateUrbanBlockHandler>();
builder.Services.AddScoped<AddBlockFaceHandler>();
builder.Services.AddScoped<CreateDistrictHandler>();
builder.Services.AddScoped<CreateNeighborhoodHandler>();
builder.Services.AddScoped<CreateFiscalSectorHandler>();
builder.Services.AddScoped<GetUrbanBlockHandler>();
builder.Services.AddScoped<SearchUrbanBlocksHandler>();

builder.Services.AddScoped<CreatePropertyHandler>();
builder.Services.AddScoped<UpdatePropertyGeometryHandler>();
builder.Services.AddScoped<AddOwnershipHandler>();
builder.Services.AddScoped<GetPropertyHandler>();
builder.Services.AddScoped<SearchPropertiesHandler>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<StorageSchemaMigrator>().MigrateAsync();
}

await host.RunAsync();