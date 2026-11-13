namespace GISORA.ArchitectureTests;

public class LayerRulesTests
{
    private static readonly string[] TechnologyLeaks =
    [
        "Microsoft.JSInterop",
        "Microsoft.AspNetCore",
        "System.Text.Json",
        "Radzen",
    ];

    [Fact]
    public void DomainDoesNotDependOnOtherLayersOrTechnology()
    {
        var domainTypes = ArchitectureModel.TypesStartingWith("GISORA.Domain");

        DependencyAssertions.AssertNoDependencyOn(
            domainTypes,
            ["GISORA.Application", "GISORA.Infrastructure", "GISORA.Web", .. TechnologyLeaks],
            "Domain");
    }

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructureWebOrTechnology()
    {
        var applicationTypes = ArchitectureModel.TypesStartingWith("GISORA.Application");

        DependencyAssertions.AssertNoDependencyOn(
            applicationTypes,
            ["GISORA.Infrastructure", "GISORA.Web", .. TechnologyLeaks],
            "Application");
    }

    [Fact]
    public void InfrastructureDoesNotDependOnHost()
    {
        var infrastructureTypes = ArchitectureModel.TypesStartingWith("GISORA.Infrastructure");

        DependencyAssertions.AssertNoDependencyOn(infrastructureTypes, ["GISORA.Web"], "Infrastructure");
    }

    [Fact]
    public void WebTypesExceptCompositionRootDoNotTouchInfrastructure()
    {
        var webTypes = ArchitectureModel.TypesStartingWith("GISORA.Web")
            .Where(type => type.FullName != "GISORA.Web.Program")
            .ToArray();

        DependencyAssertions.AssertNoDependencyOn(webTypes, ["GISORA.Infrastructure"], "Web");
    }
}