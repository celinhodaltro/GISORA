using GISORA.BuildingBlocks.Domain;
using GISORA.Cemetery.Domain;
using GISORA.Geospatial.Domain;

using CemeteryAggregate = GISORA.Cemetery.Domain.Cemetery;

namespace GISORA.Cemetery.UnitTests;

internal static class CemeteryFactory
{
    public static CemeteryAggregate New(string name = "Cemiterio Municipal")
        => CemeteryAggregate.Register(name, Square(-35.25, -7.95), Area.FromSquareMeters(10_000m));

    public static CemeterySector NewSector(CemeteryAggregate cemetery)
        => CemeterySector.Create(cemetery.Id, "A");

    public static Grave NewGrave(CemeteryAggregate cemetery, CemeterySector sector)
        => Grave.Create(cemetery.Id, sector.Id, "J-01", new Point(new Coordinate(-35.25, -7.95, SpatialReference.Wgs84)));

    public static Polygon Square(double longitude, double latitude)
    {
        const double size = 0.0005;

        return new Polygon([
            new CoordinateSequence([
                new Coordinate(longitude, latitude, SpatialReference.Wgs84),
                new Coordinate(longitude + size, latitude, SpatialReference.Wgs84),
                new Coordinate(longitude + size, latitude + size, SpatialReference.Wgs84),
                new Coordinate(longitude, latitude + size, SpatialReference.Wgs84),
                new Coordinate(longitude, latitude, SpatialReference.Wgs84),
            ]),
        ]);
    }
}

public class CemeteryDomainTests
{
    [Fact]
    public void CemeteryRequiresPositiveArea()
    {
        var exception = Assert.Throws<DomainException>(() => CemeteryAggregate.Register(
            "Vazio",
            CemeteryFactory.Square(-35.25, -7.95),
            Area.Zero));

        Assert.Equal("cemetery.area", exception.Code);
    }

    [Fact]
    public void GraveRejectsSecondActiveBurial()
    {
        var cemetery = CemeteryFactory.New();
        var grave = CemeteryFactory.NewGrave(cemetery, CemeteryFactory.NewSector(cemetery));
        grave.AddBurial("Maria", new DateOnly(2026, 1, 1));

        var exception = Assert.Throws<DomainException>(() =>
            grave.AddBurial("Joao", new DateOnly(2026, 2, 1)));

        Assert.Equal("cemetery.grave.occupied", exception.Code);
    }

    [Fact]
    public void GraveRejectsFutureBurial()
    {
        var cemetery = CemeteryFactory.New();
        var grave = CemeteryFactory.NewGrave(cemetery, CemeteryFactory.NewSector(cemetery));

        var exception = Assert.Throws<DomainException>(() =>
            grave.AddBurial("Maria", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1)));

        Assert.Equal("cemetery.burial.future-date", exception.Code);
    }

    [Fact]
    public void ExhumationFreesGrave()
    {
        var cemetery = CemeteryFactory.New();
        var grave = CemeteryFactory.NewGrave(cemetery, CemeteryFactory.NewSector(cemetery));
        var burial = grave.AddBurial("Maria", new DateOnly(2026, 1, 1));

        grave.Exhume(burial.Id, new DateOnly(2026, 3, 1));

        Assert.False(grave.IsOccupied);
        Assert.Null(grave.ActiveBurial);
    }

    [Fact]
    public void ExhumationBeforeBurialIsRejected()
    {
        var cemetery = CemeteryFactory.New();
        var grave = CemeteryFactory.NewGrave(cemetery, CemeteryFactory.NewSector(cemetery));
        var burial = grave.AddBurial("Maria", new DateOnly(2026, 5, 1));

        var exception = Assert.Throws<DomainException>(() =>
            grave.Exhume(burial.Id, new DateOnly(2026, 4, 1)));

        Assert.Equal("cemetery.burial.exhumation-date", exception.Code);
    }

    [Fact]
    public void ExhumingUnknownBurialIsRejected()
    {
        var cemetery = CemeteryFactory.New();
        var grave = CemeteryFactory.NewGrave(cemetery, CemeteryFactory.NewSector(cemetery));

        var exception = Assert.Throws<DomainException>(() =>
            grave.Exhume(Guid.CreateVersion7(), new DateOnly(2026, 1, 1)));

        Assert.Equal("cemetery.burial.not-found", exception.Code);
    }
}