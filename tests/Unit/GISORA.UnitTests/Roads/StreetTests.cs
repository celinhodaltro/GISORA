using GISORA.Domain.Common;
using GISORA.Domain.Roads;

namespace GISORA.UnitTests.Roads;

public class StreetTests
{
    [Fact]
    public void CreateRaisesEventAndTrimsName()
    {
        var street = Street.Create("  Rua da Matriz  ", StreetType.Street);

        Assert.Equal("Rua da Matriz", street.Name);
        var domainEvent = Assert.IsType<StreetRegistered>(Assert.Single(street.DomainEvents));
        Assert.Equal(street.Id, domainEvent.StreetId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsBlankName(string name)
    {
        Assert.Throws<DomainException>(() => Street.Create(name, StreetType.Street));
    }

    [Fact]
    public void RejectsNameLongerThanLimit()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Street.Create(new string('a', 121), StreetType.Street));

        Assert.Equal("roads.street.name-length", exception.Code);
    }

    [Fact]
    public void KeepsOptionalNeighborhood()
    {
        var neighborhoodId = Guid.CreateVersion7();

        var street = Street.Create("Avenida Central", StreetType.Avenue, neighborhoodId);

        Assert.Equal(neighborhoodId, street.NeighborhoodId);
    }

    [Fact]
    public void RenameValidatesInvariant()
    {
        var street = Street.Create("Rua A", StreetType.Street);

        Assert.Throws<DomainException>(() => street.Rename(" "));
    }
}