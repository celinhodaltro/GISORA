using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Infrastructure.LocalStorage;
using GISORA.Properties.Domain;
using GISORA.Territory.Domain;

namespace GISORA.ArchitectureTests;

internal static class ArchitectureModel
{
    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Entity<>).Assembly,
            typeof(Result<>).Assembly,
            typeof(Geospatial.Domain.Coordinate).Assembly,
            typeof(Geospatial.Application.IGeometryValidator).Assembly,
            typeof(Municipality).Assembly,
            typeof(Territory.Application.UrbanBlockFilter).Assembly,
            typeof(Property).Assembly,
            typeof(Properties.Application.PropertyFilter).Assembly,
            typeof(BrowserLocalStore).Assembly,
            typeof(GISORA.Infrastructure.Geospatial.GeospatialInterop).Assembly,
            typeof(Shared.DesignSystem.Components.StatCard).Assembly,
            typeof(Web.App).Assembly)
        .Build();

    public static IReadOnlyList<IType> TypesStartingWith(string prefix)
        => Architecture.Types
            .Where(type => type.FullName.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();
}