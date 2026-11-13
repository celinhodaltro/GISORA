namespace GISORA.ArchitectureTests;

public class ModuleBoundaryTests
{
    private static readonly string[] BusinessModules =
    [
        "Territory",
        "Properties",
        "Roads",
        "PublicFacilities",
        "Cemetery",
        "Rural",
        "UrbanPlanning",
        "Integration",
    ];


    [Fact]
    public void BusinessContextsDoNotDependOnEachOther()
    {
        foreach (var module in BusinessModules)
        {
            var types = ArchitectureModel.TypesStartingWith($"GISORA.Domain.{module}")
                .Concat(ArchitectureModel.TypesStartingWith($"GISORA.Application.{module}"))
                .ToArray();

            string[] forbidden =
            [
                "GISORA.Infrastructure",
                "GISORA.Web",
                .. BusinessModules
                    .Where(other => other != module)
                    .SelectMany(other => new[] { $"GISORA.Domain.{other}", $"GISORA.Application.{other}" }),
            ];

            DependencyAssertions.AssertNoDependencyOn(types, forbidden, module);
        }
    }

    [Fact]
    public void GeospatialKernelDoesNotDependOnBusinessContexts()
    {
        var types = ArchitectureModel.TypesStartingWith("GISORA.Domain.Geospatial")
            .Concat(ArchitectureModel.TypesStartingWith("GISORA.Application.Geospatial"))
            .ToArray();

        string[] forbidden =
        [
            "GISORA.Infrastructure",
            "GISORA.Web",
            .. BusinessModules.SelectMany(module => new[] { $"GISORA.Domain.{module}", $"GISORA.Application.{module}" }),
        ];

        DependencyAssertions.AssertNoDependencyOn(types, forbidden, "Geospatial");
    }
}