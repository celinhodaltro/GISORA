namespace GISORA.ArchitectureTests;

public class ModuleBoundaryTests
{
    private static readonly string[] SharedLayers =
    [
        "GISORA.Infrastructure",
        "GISORA.Shared",
        "GISORA.Web",
    ];

    [Fact]
    public void TerritoryDoesNotDependOnProperties()
    {
        var types = ArchitectureModel.TypesStartingWith("GISORA.Territory");

        DependencyAssertions.AssertNoDependencyOn(types, ["GISORA.Properties", .. SharedLayers], "Territory");
    }

    [Fact]
    public void PropertiesDoesNotDependOnTerritory()
    {
        var types = ArchitectureModel.TypesStartingWith("GISORA.Properties");

        DependencyAssertions.AssertNoDependencyOn(types, ["GISORA.Territory", .. SharedLayers], "Properties");
    }

    [Fact]
    public void GeospatialKernelDoesNotDependOnBusinessContexts()
    {
        var types = ArchitectureModel.TypesStartingWith("GISORA.Geospatial");

        DependencyAssertions.AssertNoDependencyOn(
            types,
            ["GISORA.Territory", "GISORA.Properties", .. SharedLayers],
            "Geospatial");
    }

    [Fact]
    public void RoadsDoesNotDependOnOtherBusinessContexts()
    {
        var types = ArchitectureModel.TypesStartingWith("GISORA.Roads");

        DependencyAssertions.AssertNoDependencyOn(
            types,
            ["GISORA.Territory", "GISORA.Properties", .. SharedLayers],
            "Roads");
    }

    [Fact]
    public void ModulesDoNotDependOnInfrastructureOrHosts()
    {
        foreach (var module in new[] { "GISORA.Territory", "GISORA.Properties", "GISORA.Geospatial", "GISORA.Roads", "GISORA.PublicFacilities", "GISORA.Cemetery", "GISORA.Rural", "GISORA.UrbanPlanning", "GISORA.Integration" })
        {
            var types = ArchitectureModel.TypesStartingWith(module);

            DependencyAssertions.AssertNoDependencyOn(types, SharedLayers, module);
        }
    }
}