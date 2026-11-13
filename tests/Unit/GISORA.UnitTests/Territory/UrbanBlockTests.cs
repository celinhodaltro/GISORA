using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;
using GISORA.Domain.Territory;

namespace GISORA.UnitTests.Territory;

internal static class Geo
{
    public static LineString Segment(double x1, double y1, double x2, double y2)
        => new(new CoordinateSequence([
            new Coordinate(x1, y1, SpatialReference.Wgs84),
            new Coordinate(x2, y2, SpatialReference.Wgs84),
        ]));
}

public class UrbanBlockTests
{
    private static readonly FiscalSectorId Sector = FiscalSectorId.New();

    [Fact]
    public void CreateRaisesEventAndStartsWithoutFaces()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");

        Assert.Empty(block.Faces);
        Assert.Single(block.DomainEvents);
        Assert.IsType<UrbanBlockCreated>(block.DomainEvents.First());
    }

    [Fact]
    public void RejectsBlankCode()
    {
        Assert.Throws<DomainException>(() => UrbanBlock.Create(Sector, "  "));
    }

    [Fact]
    public void AddsFace()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");
        var streetId = Guid.CreateVersion7();

        var face = block.AddFace(streetId, Geo.Segment(0, 0, 10, 0), Distance.FromMeters(10));

        Assert.Single(block.Faces);
        Assert.Equal(streetId, face.StreetId);
        Assert.IsType<BlockFaceAdded>(block.DomainEvents.Last());
    }

    [Fact]
    public void RejectsDuplicateStreetInSameBlock()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");
        var streetId = Guid.CreateVersion7();
        block.AddFace(streetId, Geo.Segment(0, 0, 10, 0), Distance.FromMeters(10));

        var exception = Assert.Throws<DomainException>(() =>
            block.AddFace(streetId, Geo.Segment(0, 10, 10, 10), Distance.FromMeters(10)));

        Assert.Equal("territory.blockface.duplicate-street", exception.Code);
    }

    [Fact]
    public void RejectsDuplicateGeometryInSameBlock()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");
        var geometry = Geo.Segment(0, 0, 10, 0);
        block.AddFace(Guid.CreateVersion7(), geometry, Distance.FromMeters(10));

        var exception = Assert.Throws<DomainException>(() =>
            block.AddFace(Guid.CreateVersion7(), Geo.Segment(0, 0, 10, 0), Distance.FromMeters(10)));

        Assert.Equal("territory.blockface.duplicate-geometry", exception.Code);
    }

    [Fact]
    public void RejectsZeroFrontage()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");

        var exception = Assert.Throws<DomainException>(() =>
            block.AddFace(Guid.CreateVersion7(), Geo.Segment(0, 0, 10, 0), Distance.Zero));

        Assert.Equal("territory.blockface.frontage", exception.Code);
    }

    [Fact]
    public void RejectsEmptyStreetId()
    {
        var block = UrbanBlock.Create(Sector, "Q-01");

        Assert.Throws<DomainException>(() =>
            block.AddFace(Guid.Empty, Geo.Segment(0, 0, 10, 0), Distance.FromMeters(10)));
    }
}