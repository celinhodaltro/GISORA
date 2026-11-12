using GISORA.BuildingBlocks.Domain;
using GISORA.Territory.Domain;

namespace GISORA.Territory.UnitTests;

public class MunicipalityTests
{
    [Fact]
    public void RegisterRaisesDomainEvent()
    {
        var municipality = Municipality.Register("Passira", IbgeCode.Parse("2611903"));

        var domainEvent = Assert.IsType<MunicipalityRegistered>(Assert.Single(municipality.DomainEvents));
        Assert.Equal(municipality.Id, domainEvent.MunicipalityId);
        Assert.Equal("Passira", domainEvent.Name);
    }

    [Fact]
    public void RegisterTrimsName()
    {
        var municipality = Municipality.Register("  Passira  ", IbgeCode.Parse("2611903"));

        Assert.Equal("Passira", municipality.Name);
    }

    [Fact]
    public void RegisterRejectsBlankName()
    {
        Assert.Throws<DomainException>(() => Municipality.Register(" ", IbgeCode.Parse("2611903")));
    }

    [Fact]
    public void RegisterRejectsMissingIbgeCode()
    {
        Assert.Throws<DomainException>(() => Municipality.Register("Passira", null!));
    }

    [Fact]
    public void BoundaryIsOptional()
    {
        var municipality = Municipality.Register("Passira", IbgeCode.Parse("2611903"));

        Assert.Null(municipality.Boundary);
    }
}