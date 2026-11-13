using GISORA.Application.Properties.Commands;
using GISORA.Application.PublicFacilities.Commands;
using GISORA.Application.Roads.Commands;
using GISORA.Application.Territory;
using GISORA.Application.Territory.Commands;
using GISORA.Infrastructure.Persistence.Demo;
using GISORA.Infrastructure.Persistence.Repositories;
using GISORA.UnitTests.Infrastructure.LocalStorage.Fakes;

namespace GISORA.UnitTests.Infrastructure.LocalStorage;

public class DemoDataSeederTests
{
    [Fact]
    public async Task SeedsPassiraWithOfficialIbgeCode()
    {
        var store = new InMemoryLocalStore();

        var seeder = new DemoDataSeeder(
            store,
            new CreateMunicipalityHandler(new LocalStorageMunicipalityRepository(store)),
            new CreateDistrictHandler(new LocalStorageDistrictRepository(store)),
            new CreateNeighborhoodHandler(new LocalStorageNeighborhoodRepository(store)),
            new CreateFiscalSectorHandler(new LocalStorageFiscalSectorRepository(store)),
            new CreateUrbanBlockHandler(new LocalStorageUrbanBlockRepository(store)),
            new CreateStreetHandler(new LocalStorageStreetRepository(store)),
            new CreatePropertyHandler(new LocalStoragePropertyRepository(store)),
            new CreateFacilityHandler(new LocalStorageFacilityRepository(store)));

        var result = await seeder.SeedAsync();

        Assert.False(result.AlreadySeeded);

        var municipalities = await new LocalStorageTerritoryQueries(store).ListMunicipalitiesAsync();
        var municipality = Assert.Single(municipalities);

        Assert.Equal("Passira", municipality.Name);
        Assert.Equal("2610509", municipality.IbgeCode);
    }

    [Fact]
    public async Task SecondSeedIsIdempotent()
    {
        var store = new InMemoryLocalStore();

        var seeder = new DemoDataSeeder(
            store,
            new CreateMunicipalityHandler(new LocalStorageMunicipalityRepository(store)),
            new CreateDistrictHandler(new LocalStorageDistrictRepository(store)),
            new CreateNeighborhoodHandler(new LocalStorageNeighborhoodRepository(store)),
            new CreateFiscalSectorHandler(new LocalStorageFiscalSectorRepository(store)),
            new CreateUrbanBlockHandler(new LocalStorageUrbanBlockRepository(store)),
            new CreateStreetHandler(new LocalStorageStreetRepository(store)),
            new CreatePropertyHandler(new LocalStoragePropertyRepository(store)),
            new CreateFacilityHandler(new LocalStorageFacilityRepository(store)));

        await seeder.SeedAsync();
        var second = await seeder.SeedAsync();

        Assert.True(second.AlreadySeeded);
    }
}