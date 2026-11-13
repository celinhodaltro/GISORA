using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using GISORA.Application.Common;
using GISORA.Domain.Common;

namespace GISORA.ArchitectureTests;

internal static class ArchitectureModel
{
    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Entity<>).Assembly,
            typeof(Result<>).Assembly,
            typeof(Infrastructure.Persistence.BrowserLocalStore).Assembly,
            typeof(Web.App).Assembly)
        .Build();

    public static IReadOnlyList<IType> TypesStartingWith(string prefix)
        => Architecture.Types
            .Where(type => type.FullName.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();
}