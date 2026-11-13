using GISORA.Domain.Common;

namespace GISORA.UnitTests.BuildingBlocks;

public class EntityTests
{
    private sealed class Property(Guid id) : Entity<Guid>(id);

    private sealed class Taxpayer(Guid id) : Entity<Guid>(id);

    private sealed class Named(string id) : Entity<string>(id);

    [Fact]
    public void EntitiesOfSameTypeWithSameIdAreEqual()
    {
        var id = Guid.CreateVersion7();

        Assert.Equal(new Property(id), new Property(id));
    }

    [Fact]
    public void EntitiesOfDifferentTypesWithSameIdAreNotEqual()
    {
        var id = Guid.CreateVersion7();

        Assert.NotEqual<Entity<Guid>>(new Property(id), new Taxpayer(id));
    }

    [Fact]
    public void EntityRejectsNullIdentity()
    {
        Assert.Throws<ArgumentNullException>(() => new Named(null!));
    }
}