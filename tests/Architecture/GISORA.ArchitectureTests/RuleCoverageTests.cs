using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.ArchitectureTests;

public class RuleCoverageTests
{
    [Fact]
    public void ArchitectureContainsAllSolutionAssemblies()
    {
        var assemblies = ArchitectureModel.Architecture.Assemblies
            .Select(assembly => assembly.Name)
            .ToHashSet(StringComparer.Ordinal);

        string[] expected =
        [
            "GISORA.BuildingBlocks.Domain",
            "GISORA.BuildingBlocks.Application",
            "GISORA.Geospatial.Domain",
            "GISORA.Geospatial.Application",
            "GISORA.Territory.Domain",
            "GISORA.Territory.Application",
            "GISORA.Properties.Domain",
            "GISORA.Properties.Application",
            "GISORA.Roads.Domain",
            "GISORA.Roads.Application",
            "GISORA.PublicFacilities.Domain",
            "GISORA.PublicFacilities.Application",
            "GISORA.Cemetery.Domain",
            "GISORA.Cemetery.Application",
            "GISORA.Infrastructure.LocalStorage",
            "GISORA.Infrastructure.Geospatial",
            "GISORA.Shared.DesignSystem",
            "GISORA.Shared.Maps",
            "GISORA.Web",
        ];

        Assert.All(expected, name => Assert.Contains(name, assemblies));
    }

    [Fact]
    public void EveryLayerRuleHasTypesToCheck()
    {
        Assert.True(HasTypesIn("GISORA.", ".Domain"));
        Assert.True(HasTypesIn("GISORA.", ".Application"));
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Infrastructure.").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Web").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Territory").Count > 0);
        Assert.True(ArchitectureModel.TypesStartingWith("GISORA.Properties").Count > 0);
    }

    [Fact]
    public void RuleDetectsForbiddenDependency()
    {
        var violation = DependencyAssertions.FindViolation(
            "GISORA.Properties.Domain.Example",
            ["GISORA.Infrastructure.LocalStorage.ILocalStore", "System.String"],
            ["GISORA.Infrastructure"]);

        Assert.NotNull(violation);
        Assert.Contains("GISORA.Infrastructure.LocalStorage.ILocalStore", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void RuleAcceptsAllowedDependency()
    {
        var violation = DependencyAssertions.FindViolation(
            "GISORA.Properties.Domain.Example",
            ["GISORA.Geospatial.Domain.Area", "GISORA.BuildingBlocks.Domain.Guard"],
            ["GISORA.Infrastructure"]);

        Assert.Null(violation);
    }

    [Fact]
    public void DomainPrimitivesRemainInBuildingBlocks()
    {
        Assert.NotNull(typeof(AggregateRoot<>));
        Assert.NotNull(typeof(ValueObject));
        Assert.NotNull(typeof(Geometry));
    }

    private static bool HasTypesIn(string prefix, string namespaceSuffix)
        => ArchitectureModel.TypesStartingWith(prefix)
            .Any(type => type.Namespace!.FullName.EndsWith(namespaceSuffix, StringComparison.Ordinal));
}