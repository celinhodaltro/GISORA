namespace GISORA.ArchitectureTests;

public class LayerRulesTests
{
    private static readonly string[] InfrastructureAndHosts =
    [
        "GISORA.Infrastructure",
        "GISORA.Web",
        "GISORA.Shared.DesignSystem",
    ];

    private static readonly string[] TechnologyLeaks =
    [
        "Microsoft.JSInterop",
        "Microsoft.AspNetCore",
        "System.Text.Json",
        "Radzen",
    ];

    [Fact]
    public void DomainDoesNotDependOnInfrastructureHostsOrTechnology()
    {
        var domainTypes = ArchitectureModel.TypesStartingWith("GISORA.")
            .Where(type => type.Namespace!.FullName.EndsWith(".Domain", StringComparison.Ordinal))
            .ToArray();

        DependencyAssertions.AssertNoDependencyOn(domainTypes, InfrastructureAndHosts, "Domain");
        DependencyAssertions.AssertNoDependencyOn(domainTypes, TechnologyLeaks, "Domain");
    }

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructureOrTechnology()
    {
        var applicationTypes = ArchitectureModel.TypesStartingWith("GISORA.")
            .Where(type => type.Namespace!.FullName.EndsWith(".Application", StringComparison.Ordinal))
            .ToArray();

        DependencyAssertions.AssertNoDependencyOn(applicationTypes, InfrastructureAndHosts, "Application");
        DependencyAssertions.AssertNoDependencyOn(applicationTypes, TechnologyLeaks, "Application");
    }

    [Fact]
    public void InfrastructureDoesNotDependOnHosts()
    {
        var infrastructureTypes = ArchitectureModel.TypesStartingWith("GISORA.Infrastructure.");

        DependencyAssertions.AssertNoDependencyOn(infrastructureTypes, ["GISORA.Web", "GISORA.Shared.DesignSystem"], "Infrastructure");
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