using ArchUnitNET.Domain;

namespace GISORA.ArchitectureTests;

internal static class DependencyAssertions
{
    public static void AssertNoDependencyOn(
        IEnumerable<IType> types,
        IEnumerable<string> forbiddenNamespacePrefixes,
        string scopeDescription)
    {
        var forbidden = forbiddenNamespacePrefixes.ToArray();

        foreach (var type in types)
        {
            var namespaceName = type.Namespace?.FullName ?? string.Empty;
            if (namespaceName.StartsWith("System", StringComparison.Ordinal))
            {
                continue;
            }

            var targetNames = type.Dependencies
                .Select(dependency => dependency.Target)
                .OfType<IType>()
                .Select(target => target.FullName);

            var violation = FindViolation(type.FullName, targetNames, forbidden);

            Assert.True(violation is null, $"{scopeDescription}: {violation}");
        }
    }

    public static string? FindViolation(string sourceFullName, IEnumerable<string> targetFullNames, string[] forbiddenPrefixes)
    {
        var violations = targetFullNames
            .Where(target => forbiddenPrefixes.Any(prefix => target.StartsWith(prefix, StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return violations.Length == 0
            ? null
            : $"'{sourceFullName}' depende de {string.Join(", ", violations)}.";
    }
}