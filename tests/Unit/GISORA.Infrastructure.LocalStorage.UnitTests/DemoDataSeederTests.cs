using GISORA.Infrastructure.LocalStorage.Demo;
using GISORA.Infrastructure.LocalStorage.Repositories;
using GISORA.Infrastructure.LocalStorage.UnitTests.Fakes;
using GISORA.Properties.Application.Commands;
using GISORA.PublicFacilities.Application.Commands;
using GISORA.Roads.Application.Commands;
using GISORA.Territory.Application;
using GISORA.Territory.Application.Commands;

namespace GISORA.Infrastructure.LocalStorage.UnitTests;

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