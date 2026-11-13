using GISORA.Application.Rural.Commands;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Rural;

namespace GISORA.UnitTests.Rural;

public class RuralPropertyTests
{
    [Fact]
    public void AppLargerThanPropertyIsRejected()
    {
        var property = RuralFactory.New();
        var app = PreservationArea.Create("Mata ciliar", RuralFactory.Square(), Area.FromHectares(150m));

        var exception = Assert.Throws<DomainException>(() => property.AddPreservationArea(app));

        Assert.Equal("rural.app.exceeds-property", exception.Code);
    }

    [Fact]
    public void ConstraintsCannotExceedPropertyArea()
    {
        var property = RuralFactory.New(constrained: 60m);
        var app = PreservationArea.Create("Mata ciliar", RuralFactory.Square(), Area.FromHectares(60m));

        var exception = Assert.Throws<DomainException>(() => property.AddPreservationArea(app));

        Assert.Equal("rural.constraints.exceeds-property", exception.Code);
    }

    [Fact]
    public void LegalReserveLargerThanPropertyIsRejected()
    {
        var property = RuralFactory.New();

        var exception = Assert.Throws<DomainException>(
            () => property.DefineLegalReserve(RuralFactory.Square(), Area.FromHectares(120m)));

        Assert.Equal("rural.legal-reserve.exceeds-property", exception.Code);
    }

    [Fact]
    public void CodesAreNormalizedAndSpacesRejected()
    {
        var property = RuralProperty.Register(
            "Fazenda",
            RuralFactory.Square(),
            Area.FromHectares(10m),
            RuralUsage.Mixed,
            carCode: " pe-2610509-abcd ",
            sigefCode: "");

        Assert.Equal("PE-2610509-ABCD", property.CarCode);
        Assert.Null(property.SigefCode);

        var exception = Assert.Throws<DomainException>(() => RuralProperty.Register(
            "Fazenda",
            RuralFactory.Square(),
            Area.FromHectares(10m),
            RuralUsage.Mixed,
            carCode: "PE 123"));

        Assert.Equal("rural.code.whitespace", exception.Code);
    }

    [Fact]
    public void ConstrainedAreaSumsReserveAndPreservationAreas()
    {
        var property = RuralFactory.New(constrained: 20m);
        property.AddPreservationArea(PreservationArea.Create("Nascente", RuralFactory.Square(), Area.FromHectares(5m)));

        Assert.Equal(250_000m, property.ConstrainedSquareMeters);
    }
}

public class RuralHandlerTests
{
    [Fact]
    public async Task ImportRecordsSourceCrsAndComputedArea()
    {
        var repository = new InMemoryRuralRepository();
        var handler = new ImportRuralPropertyHandler(
            repository,
            new FakeGeoJsonSerializer(RuralFactory.Square()),
            new FakeGeometryValidator(),
            new FakeSpatialOperations { AreaSquareMeters = 500_000m });

        var result = await handler.HandleAsync(new ImportRuralPropertyCommand(
            "Fazenda Importada",
            "{}",
            RuralUsage.Forestry));

        Assert.True(result.IsSuccess);

        var property = Assert.Single(repository.Items);
        Assert.Equal(500_000m, property.Area.SquareMeters);
        Assert.Equal("GeoJson", property.Source.Kind);
        Assert.Equal("EPSG:4326", property.Source.Crs);
    }

    [Fact]
    public async Task ImportRejectsInvalidGeometry()
    {
        var handler = new ImportRuralPropertyHandler(
            new InMemoryRuralRepository(),
            new FakeGeoJsonSerializer(RuralFactory.Square()),
            new FakeGeometryValidator(isValid: false),
            new FakeSpatialOperations());

        var result = await handler.HandleAsync(new ImportRuralPropertyCommand(
            "Fazenda Importada",
            "{}",
            RuralUsage.Forestry));

        Assert.True(result.IsFailure);
        Assert.Equal("rural.import.invalid-geometry", result.Error.Code);
    }

    [Fact]
    public async Task PreservationAreaOverlappingLegalReserveIsRejected()
    {
        var repository = new InMemoryRuralRepository();
        var property = RuralFactory.New(constrained: 20m);
        await repository.AddAsync(property);

        var spatial = new FakeSpatialOperations
        {
            IntersectsResult = true,
            IntersectionResult = RuralFactory.Square(),
        };
        spatial.WithArea(50_000m).WithArea(10_000m);

        var handler = new AddPreservationAreaHandler(repository, spatial);

        var result = await handler.HandleAsync(new AddPreservationAreaCommand(
            property.Id,
            "Mata ciliar",
            RuralFactory.Square()));

        Assert.True(result.IsFailure);
        Assert.Equal("rural.app.legal-reserve-overlap", result.Error.Code);
    }

    [Fact]
    public async Task PreservationAreaWithinPropertyIsAdded()
    {
        var repository = new InMemoryRuralRepository();
        var property = RuralFactory.New();
        await repository.AddAsync(property);

        var spatial = new FakeSpatialOperations { IntersectsResult = false };
        spatial.WithArea(50_000m);

        var handler = new AddPreservationAreaHandler(repository, spatial);

        var result = await handler.HandleAsync(new AddPreservationAreaCommand(
            property.Id,
            "Mata ciliar",
            RuralFactory.Square()));

        Assert.True(result.IsSuccess);
        Assert.Single(property.PreservationAreas);
    }
}