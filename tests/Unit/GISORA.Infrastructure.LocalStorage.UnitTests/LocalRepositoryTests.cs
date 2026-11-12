using System.Globalization;

using GISORA.Geospatial.Domain;
using GISORA.Infrastructure.LocalStorage.Persistence;
using GISORA.Infrastructure.LocalStorage.Repositories;
using GISORA.Infrastructure.LocalStorage.UnitTests.Fakes;
using GISORA.Properties.Application;
using GISORA.Properties.Domain;
using GISORA.Territory.Application;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.UnitTests;

public class LocalRepositoryTests
{
    private static Polygon Square(double size, GeoSize? offset = null)
    {
        var origin = offset ?? GeoSize.Origin;

        return new([
            new CoordinateSequence([
                new Coordinate(origin.X, origin.Y, SpatialReference.Wgs84),
                new Coordinate(origin.X + size, origin.Y, SpatialReference.Wgs84),
                new Coordinate(origin.X + size, origin.Y + size, SpatialReference.Wgs84),
                new Coordinate(origin.X, origin.Y + size, SpatialReference.Wgs84),
                new Coordinate(origin.X, origin.Y, SpatialReference.Wgs84),
            ]),
        ]);
    }

    private static Property NewProperty(string lot = "003", Guid? neighborhoodId = null)
        => Property.Register(
            PropertyRegistration.Create("001", "002", lot),
            new Address(Guid.CreateVersion7(), "10", neighborhoodId ?? Guid.CreateVersion7()),
            Area.FromSquareMeters(500m),
            Square(20));

    [Fact]
    public async Task PropertyRoundTripPreservesChildrenAndGeometry()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStoragePropertyRepository(store);
        var property = NewProperty();
        property.AddBuilding(BuildingType.Residential, Area.FromSquareMeters(120m), 2);
        property.AddUnit("101", Area.FromSquareMeters(60m), UnitUsage.Residential);
        var taxpayerId = TaxpayerId.New();
        property.AddOwnership(taxpayerId, OwnershipShare.FromPercentage(100m), new DateOnly(2026, 1, 1));
        property.ChangeGeometry(Square(22, new GeoSize(1, 1)), "retificacao");

        await repository.AddAsync(property);
        var loaded = await repository.FindAsync(property.Id);

        Assert.NotNull(loaded);
        Assert.Equal(property.Registration, loaded.Registration);
        Assert.Equal(property.Address, loaded.Address);
        Assert.Equal(500m, loaded.LandArea.SquareMeters);
        Assert.Single(loaded.Buildings);
        Assert.Single(loaded.Units);
        Assert.Single(loaded.Ownerships);
        Assert.Equal(taxpayerId, loaded.Ownerships.First().TaxpayerId);
        Assert.Single(loaded.History);
        Assert.Equal(5, loaded.Geometry!.Coordinates.Count);
        Assert.Equal(2, loaded.Buildings.First().Floors);
    }

    [Fact]
    public async Task PropertyIndexIsUpdatedOnAddAndUpdate()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStoragePropertyRepository(store);
        var queries = new LocalStoragePropertyQueries(store);
        var property = NewProperty("100");
        await repository.AddAsync(property);

        var initial = await queries.SearchAsync(new PropertyFilter());
        Assert.Single(initial.Items);

        property.AddBuilding(BuildingType.Commercial, Area.FromSquareMeters(80m), 1);
        await repository.UpdateAsync(property);

        var updated = await queries.SearchAsync(new PropertyFilter());
        Assert.Equal(80m, updated.Items.Single().BuiltAreaSquareMeters);
        Assert.Equal(1, updated.Items.Single().BuildingCount);
        Assert.Equal(1, updated.TotalCount);
    }

    [Fact]
    public async Task PropertySearchReadsOnlyTheIndex()
    {
        var store = new CountingLocalStore(new InMemoryLocalStore());
        var properties = new List<Property>();

        for (var index = 0; index < 1_000; index++)
        {
            var property = NewProperty(index.ToString("D6", CultureInfo.InvariantCulture));
            properties.Add(property);
            await store.SetAsync(
                StorageNamespaces.Properties,
                $"property-{property.Id.Value}",
                PropertyDocument.FromDomain(property),
                CancellationToken.None);
        }

        await store.SetAsync(
            StorageNamespaces.Properties,
            "property-index",
            properties.Select(PropertyIndexEntry.FromDomain).ToList(),
            CancellationToken.None);

        var queries = new LocalStoragePropertyQueries(store);
        store.ResetCounters();

        var page = await queries.SearchAsync(new PropertyFilter { PageNumber = 1, PageSize = 25 });

        Assert.Equal(1_000, page.TotalCount);
        Assert.Equal(25, page.Items.Count);
        Assert.Equal(1, store.GetCount);
    }

    [Fact]
    public async Task PropertySearchFiltersAndPaginatesDeterministically()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStoragePropertyRepository(store);
        var queries = new LocalStoragePropertyQueries(store);
        var neighborhood = Guid.CreateVersion7();

        await repository.AddAsync(NewProperty("002", neighborhood));
        await repository.AddAsync(NewProperty("001", neighborhood));
        await repository.AddAsync(NewProperty("003"));

        var byNeighborhood = await queries.SearchAsync(new PropertyFilter { NeighborhoodId = neighborhood });
        var byRegistration = await queries.SearchAsync(new PropertyFilter { RegistrationContains = "003" });
        var firstPage = await queries.SearchAsync(new PropertyFilter { PageSize = 2 });

        Assert.Equal(2, byNeighborhood.TotalCount);
        Assert.Equal("001.002.001", byNeighborhood.Items[0].Registration);
        Assert.Single(byRegistration.Items);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal("001.002.001", firstPage.Items[0].Registration);
    }

    [Fact]
    public async Task RebuildIndexRestoresInconsistentIndex()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStoragePropertyRepository(store);
        var queries = new LocalStoragePropertyQueries(store);
        await repository.AddAsync(NewProperty("010"));

        await store.SetAsync(
            StorageNamespaces.Properties,
            "property-index",
            new List<PropertyIndexEntry>(),
            CancellationToken.None);

        Assert.Empty((await queries.SearchAsync(new PropertyFilter())).Items);

        await repository.RebuildIndexAsync();

        Assert.Single((await queries.SearchAsync(new PropertyFilter())).Items);
    }

    [Fact]
    public async Task PropertyAddRejectsDuplicatedRegistration()
    {
        var repository = new LocalStoragePropertyRepository(new InMemoryLocalStore());
        await repository.AddAsync(NewProperty());

        await Assert.ThrowsAsync<StorageException>(() => repository.AddAsync(NewProperty()));
    }

    [Fact]
    public async Task PropertyUpdateFailsWhenMissing()
    {
        var repository = new LocalStoragePropertyRepository(new InMemoryLocalStore());

        await Assert.ThrowsAsync<StorageException>(() => repository.UpdateAsync(NewProperty()));
    }

    [Fact]
    public async Task UrbanBlockRoundTripPreservesFaces()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStorageUrbanBlockRepository(store);
        var block = UrbanBlock.Create(FiscalSectorId.New(), "Q-01");
        block.AddFace(Guid.CreateVersion7(), new LineString(new CoordinateSequence([
            new Coordinate(0, 0, SpatialReference.Wgs84),
            new Coordinate(10, 0, SpatialReference.Wgs84),
        ])), Distance.FromMeters(10));

        await repository.AddAsync(block);
        var loaded = await repository.FindAsync(block.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Q-01", loaded.Code);
        Assert.Single(loaded.Faces);
        Assert.Equal(10m, loaded.Faces.First().Frontage.Meters);
    }

    [Fact]
    public async Task UrbanBlockQueriesFilterBySector()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStorageUrbanBlockRepository(store);
        var queries = new LocalStorageUrbanBlockQueries(store);
        var sector = FiscalSectorId.New();
        await repository.AddAsync(UrbanBlock.Create(sector, "Q-02"));
        await repository.AddAsync(UrbanBlock.Create(sector, "Q-01"));
        await repository.AddAsync(UrbanBlock.Create(FiscalSectorId.New(), "Q-03"));

        var page = await queries.SearchAsync(new UrbanBlockFilter { FiscalSectorId = sector, PageSize = 10 });

        Assert.Equal(2, page.TotalCount);
        Assert.Equal("Q-01", page.Items[0].Code);
    }

    [Fact]
    public async Task TaxpayerRoundTripKeepsDocumentValid()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStorageTaxpayerRepository(store);
        var taxpayer = Taxpayer.Register("Maria", DocumentNumber.Parse("52998224725"));

        await repository.AddAsync(taxpayer);
        var loaded = await repository.FindAsync(taxpayer.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Maria", loaded.Name);
        Assert.Equal("***.***.***-25", loaded.Document.ToString());
        Assert.True(await repository.ExistsByDocumentAsync(taxpayer.Document));
    }

    [Fact]
    public async Task TaxpayerAddRejectsDuplicatedDocument()
    {
        var repository = new LocalStorageTaxpayerRepository(new InMemoryLocalStore());
        await repository.AddAsync(Taxpayer.Register("Maria", DocumentNumber.Parse("52998224725")));

        await Assert.ThrowsAsync<StorageException>(() =>
            repository.AddAsync(Taxpayer.Register("Outra", DocumentNumber.Parse("52998224725"))));
    }

    [Fact]
    public async Task MunicipalityKeepsIbgeUniqueness()
    {
        var store = new InMemoryLocalStore();
        var repository = new LocalStorageMunicipalityRepository(store);
        var municipality = Municipality.Register("Passira", IbgeCode.Parse("2611903"));
        await repository.AddAsync(municipality);

        Assert.True(await repository.ExistsByIbgeCodeAsync(IbgeCode.Parse("2611903")));

        await Assert.ThrowsAsync<StorageException>(() =>
            repository.AddAsync(Municipality.Register("Outro", IbgeCode.Parse("2611903"))));
    }

    private readonly record struct GeoSize(double X, double Y)
    {
        public static GeoSize Origin => new(0, 0);
    }
}