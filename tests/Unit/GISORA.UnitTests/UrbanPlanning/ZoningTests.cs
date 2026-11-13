using GISORA.Application.UrbanPlanning.Commands;
using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.UrbanPlanning;

namespace GISORA.UnitTests.UrbanPlanning;

public class ZoningTests
{
    [Fact]
    public void ZoneRequiresAtLeastOneParameter()
    {
        var exception = Assert.Throws<DomainException>(() => ZoneParameters.Create());

        Assert.Equal("urban.zone.parameters", exception.Code);
    }

    [Fact]
    public void OccupancyMustBeWithinRange()
    {
        var exception = Assert.Throws<DomainException>(
            () => ZoneParameters.Create(maxOccupancyPercent: 120m));

        Assert.Equal("urban.zone.occupancy", exception.Code);
    }

    [Fact]
    public void ZoneCodeIsNormalizedAndCannotContainSpaces()
    {
        var zone = PlanningFactory.NewZone(" zr2 ");

        Assert.Equal("ZR2", zone.Code);

        var exception = Assert.Throws<DomainException>(() => PlanningFactory.NewZone("ZR 2"));

        Assert.Equal("urban.zone.code", exception.Code);
    }

    [Fact]
    public void ZoneRequiresPositiveArea()
    {
        var exception = Assert.Throws<DomainException>(() => Zone.Define(
            "ZR1",
            "Zona residencial",
            ZoneParameters.Create(maxHeightMeters: 12m),
            PlanningFactory.Square(),
            Area.Zero));

        Assert.Equal("urban.zone.area", exception.Code);
    }

    [Fact]
    public async Task RestrictionWithInvalidGeometryIsRejected()
    {
        var handler = new CreateRestrictionHandler(
            new InMemoryRestrictionRepository(),
            new FakeGeometryValidator(isValid: false),
            new FakeSpatialOperations());

        var result = await handler.HandleAsync(new CreateRestrictionCommand(
            RestrictionType.RiskArea,
            "Encosta instavel",
            PlanningFactory.Square()));

        Assert.True(result.IsFailure);
        Assert.Equal("urban.restriction.invalid-geometry", result.Error.Code);
    }

    [Fact]
    public async Task DuplicateZoneCodeIsRejected()
    {
        var repository = new InMemoryZoneRepository();
        await repository.AddAsync(PlanningFactory.NewZone("ZR1"));

        var handler = new CreateZoneHandler(repository, new FakeSpatialOperations());

        var result = await handler.HandleAsync(new CreateZoneCommand(
            "zr1",
            "Zona residencial 1",
            ZoneParameters.Create(minSetbackMeters: 3m),
            PlanningFactory.Square()));

        Assert.True(result.IsFailure);
        Assert.Equal("urban.zone.duplicated", result.Error.Code);
    }
}