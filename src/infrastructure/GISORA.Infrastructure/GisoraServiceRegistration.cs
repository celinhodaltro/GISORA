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

using Microsoft.Extensions.DependencyInjection;

namespace GISORA.Infrastructure;

public static class GisoraServiceRegistration
{
    public static IServiceCollection AddGisoraCore(this IServiceCollection services)
    {
        services.AddScoped<ILocalStore, BrowserLocalStore>();
        services.AddScoped<StorageSchemaMigrator>();

        services.AddScoped<IMunicipalityRepository, LocalStorageMunicipalityRepository>();
        services.AddScoped<IDistrictRepository, LocalStorageDistrictRepository>();
        services.AddScoped<INeighborhoodRepository, LocalStorageNeighborhoodRepository>();
        services.AddScoped<IFiscalSectorRepository, LocalStorageFiscalSectorRepository>();
        services.AddScoped<IUrbanBlockRepository, LocalStorageUrbanBlockRepository>();
        services.AddScoped<IUrbanBlockQueries, LocalStorageUrbanBlockQueries>();
        services.AddScoped<ITerritoryQueries, LocalStorageTerritoryQueries>();

        services.AddScoped<IPropertyRepository, LocalStoragePropertyRepository>();
        services.AddScoped<ITaxpayerRepository, LocalStorageTaxpayerRepository>();
        services.AddScoped<IPropertyQueries, LocalStoragePropertyQueries>();
        services.AddScoped<IPropertyMapQueries, LocalStoragePropertyMapQueries>();

        services.AddScoped<IStreetRepository, LocalStorageStreetRepository>();
        services.AddScoped<IStreetQueries, LocalStorageStreetQueries>();
        services.AddScoped<CreateStreetHandler>();

        services.AddScoped<IPublicFacilityRepository, LocalStorageFacilityRepository>();
        services.AddScoped<IFacilityQueries, LocalStorageFacilityQueries>();
        services.AddScoped<CreateFacilityHandler>();

        services.AddScoped<ICemeteryRepository, LocalStorageCemeteryRepository>();
        services.AddScoped<ICemeterySectorRepository, LocalStorageCemeterySectorRepository>();
        services.AddScoped<IGraveRepository, LocalStorageGraveRepository>();
        services.AddScoped<ICemeteryQueries, LocalStorageCemeteryQueries>();
        services.AddScoped<CreateCemeteryHandler>();
        services.AddScoped<CreateCemeterySectorHandler>();
        services.AddScoped<CreateGraveHandler>();
        services.AddScoped<RegisterBurialHandler>();
        services.AddScoped<ExhumeBurialHandler>();

        services.AddScoped<IRuralPropertyRepository, LocalStorageRuralPropertyRepository>();
        services.AddScoped<IRuralPropertyQueries, LocalStorageRuralPropertyQueries>();
        services.AddScoped<CreateRuralPropertyHandler>();
        services.AddScoped<ImportRuralPropertyHandler>();
        services.AddScoped<AddPreservationAreaHandler>();
        services.AddScoped<DefineLegalReserveHandler>();

        services.AddScoped<IZoneRepository, LocalStorageZoneRepository>();
        services.AddScoped<IRestrictionRepository, LocalStorageRestrictionRepository>();
        services.AddScoped<IRegulationQueries, LocalStorageRegulationQueries>();
        services.AddScoped<CreateZoneHandler>();
        services.AddScoped<CreateRestrictionHandler>();

        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();

        services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://servicodados.ibge.gov.br/") });
        services.AddScoped<IMunicipalityCatalog, IbgeMunicipalityCatalog>();

        services.AddSingleton<IGeoJsonSerializer, GeoJsonSerializer>();
        services.AddScoped<GeospatialInterop>();
        services.AddScoped<IGeometryValidator, TurfGeometryValidator>();
        services.AddScoped<ICoordinateTransformer, Proj4CoordinateTransformer>();
        services.AddScoped<ISpatialOperations, TurfSpatialOperations>();

        services.AddScoped<CreateMunicipalityHandler>();
        services.AddScoped<CreateUrbanBlockHandler>();
        services.AddScoped<AddBlockFaceHandler>();
        services.AddScoped<CreateDistrictHandler>();
        services.AddScoped<CreateNeighborhoodHandler>();
        services.AddScoped<CreateFiscalSectorHandler>();
        services.AddScoped<GetUrbanBlockHandler>();
        services.AddScoped<SearchUrbanBlocksHandler>();

        services.AddScoped<CreatePropertyHandler>();
        services.AddScoped<UpdatePropertyGeometryHandler>();
        services.AddScoped<AddOwnershipHandler>();
        services.AddScoped<GetPropertyHandler>();
        services.AddScoped<SearchPropertiesHandler>();

        return services;
    }
}