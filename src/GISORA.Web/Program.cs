using GISORA.Application.Cemetery;
using GISORA.Application.Cemetery.Commands;
using GISORA.Application.Common;
using GISORA.Application.Geospatial;
using GISORA.Application.Properties;
using GISORA.Application.Properties.Commands;
using GISORA.Application.Properties.Queries;
using GISORA.Application.PublicFacilities;
using GISORA.Application.PublicFacilities.Commands;
using GISORA.Application.Roads;
using GISORA.Application.Roads.Commands;
using GISORA.Application.Rural;
using GISORA.Application.Rural.Commands;
using GISORA.Application.Territory;
using GISORA.Application.Territory.Commands;
using GISORA.Application.Territory.Queries;
using GISORA.Application.UrbanPlanning;
using GISORA.Application.UrbanPlanning.Commands;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Properties;
using GISORA.Domain.PublicFacilities;
using GISORA.Domain.Roads;
using GISORA.Domain.Rural;
using GISORA.Domain.Territory;
using GISORA.Domain.UrbanPlanning;
using GISORA.Infrastructure.Geospatial;
using GISORA.Infrastructure.Integrations;
using GISORA.Infrastructure.Persistence;
using GISORA.Infrastructure.Persistence.Demo;
using GISORA.Infrastructure.Persistence.Repositories;
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
builder.Services.AddScoped<ICemeteryRepository, LocalStorageCemeteryRepository>();
builder.Services.AddScoped<ICemeterySectorRepository, LocalStorageCemeterySectorRepository>();
builder.Services.AddScoped<IGraveRepository, LocalStorageGraveRepository>();
builder.Services.AddScoped<ICemeteryQueries, LocalStorageCemeteryQueries>();
builder.Services.AddScoped<CreateCemeteryHandler>();
builder.Services.AddScoped<CreateCemeterySectorHandler>();
builder.Services.AddScoped<CreateGraveHandler>();
builder.Services.AddScoped<RegisterBurialHandler>();
builder.Services.AddScoped<ExhumeBurialHandler>();
builder.Services.AddScoped<IRuralPropertyRepository, LocalStorageRuralPropertyRepository>();
builder.Services.AddScoped<IRuralPropertyQueries, LocalStorageRuralPropertyQueries>();
builder.Services.AddScoped<CreateRuralPropertyHandler>();
builder.Services.AddScoped<ImportRuralPropertyHandler>();
builder.Services.AddScoped<AddPreservationAreaHandler>();
builder.Services.AddScoped<DefineLegalReserveHandler>();
builder.Services.AddScoped<IZoneRepository, LocalStorageZoneRepository>();
builder.Services.AddScoped<IRestrictionRepository, LocalStorageRestrictionRepository>();
builder.Services.AddScoped<IRegulationQueries, LocalStorageRegulationQueries>();
builder.Services.AddScoped<CreateZoneHandler>();
builder.Services.AddScoped<CreateRestrictionHandler>();
builder.Services.AddCibIntegration(builder.Configuration);
builder.Services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();
builder.Services.AddScoped<GISORA.Web.Layout.MunicipalityState>();

builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://servicodados.ibge.gov.br/") });
builder.Services.AddScoped<IMunicipalityCatalog, IbgeMunicipalityCatalog>();
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