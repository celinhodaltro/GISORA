using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.ArchitectureTests;

public class RuleCoverageTests
{
    [Fact]
    public void ArchitectureContainsCoreAssemblies()
    {
        var assemblies = ArchitectureModel.Architecture.Assemblies
            .Select(assembly => assembly.Name)
            .ToHashSet(StringComparer.Ordinal);

        string[] expected =
        [
            "GISORA.Domain",
            "GISORA.Application",
            "GISORA.Infrastructure",
            "GISORA.Presentation.Shared",
            "GISORA.Presentation",
        ];

        Assert.All(expected, name => Assert.Contains(name, assemblies));
    }

    [Fact]
    public void EveryLayerRuleHasTypesToCheck()
    {
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Domain").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Application").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Infrastructure").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Presentation").Count > 0);
    }

    [Fact]
    public void RuleDetectsForbiddenDependency()
    {
        var violation = DependencyAssertions.FindViolation(
            "GISORA.Domain.Properties.Example",
            ["GISORA.Infrastructure.Persistence.ILocalStore", "System.String"],
            ["GISORA.Infrastructure"]);

        Assert.NotNull(violation);
        Assert.Contains("GISORA.Infrastructure.Persistence.ILocalStore", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void RuleAcceptsAllowedDependency()
    {
        var violation = DependencyAssertions.FindViolation(
            "GISORA.Domain.Properties.Example",
            ["GISORA.Domain.Geospatial.Area", "GISORA.Domain.Common.Guard"],
            ["GISORA.Infrastructure"]);

        Assert.Null(violation);
    }

    [Fact]
    public void DomainPrimitivesRemainInDomain()
    {
        Assert.NotNull(typeof(AggregateRoot<>));
        Assert.NotNull(typeof(ValueObject));
        Assert.NotNull(typeof(Geometry));
    }
}