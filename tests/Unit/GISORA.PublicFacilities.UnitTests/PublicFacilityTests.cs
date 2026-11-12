using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;
using GISORA.PublicFacilities.Domain;

namespace GISORA.PublicFacilities.UnitTests;

public class PublicFacilityTests
{
    private static Point Location() => new(new Coordinate(-35.25, -7.95, SpatialReference.Wgs84));

    [Fact]
    public void RegisterRaisesEventAndTrimsName()
    {
        var facility = PublicFacility.Register("  Escola Municipal  ", FacilityType.School, null, Location());

        Assert.Equal("Escola Municipal", facility.Name);
        Assert.IsType<PublicFacilityRegistered>(Assert.Single(facility.DomainEvents));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsBlankName(string name)
    {
        Assert.Throws<DomainException>(() => PublicFacility.Register(name, FacilityType.School, null, null));
    }

    [Fact]
    public void RejectsNameLongerThanLimit()
    {
        var exception = Assert.Throws<DomainException>(() =>
            PublicFacility.Register(new string('a', 121), FacilityType.Park, null, null));

        Assert.Equal("facilities.name-length", exception.Code);
    }

    [Fact]
    public void LocationIsOptional()
    {
        var facility = PublicFacility.Register("Praca Central", FacilityType.Square, null, null);

        Assert.Null(facility.Location);
    }

    [Fact]
    public void RelocateRequiresLocation()
    {
        var facility = PublicFacility.Register("Praca Central", FacilityType.Square, null, null);

        Assert.Throws<DomainException>(() => facility.Relocate(null!));
    }
}