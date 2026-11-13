using GISORA.Domain.Common;

namespace GISORA.Infrastructure.Persistence;

public static class LocalStoreKeys
{
    public const string RootPrefix = "gisora";

    public static string For(string storageNamespace, string key)
    {
        var normalizedNamespace = Normalize(storageNamespace, nameof(storageNamespace));
        var normalizedKey = Normalize(key, nameof(key));

        return $"{RootPrefix}:{normalizedNamespace}:{normalizedKey}";
    }

    public static string NamespacePrefix(string storageNamespace)
    {
        var normalizedNamespace = Normalize(storageNamespace, nameof(storageNamespace));

        return $"{RootPrefix}:{normalizedNamespace}:";
    }

    private static string Normalize(string value, string parameterName)
    {
        var normalized = Guard.NotNullOrWhiteSpace(value, parameterName).Trim();

        Guard.Condition(
            !normalized.Contains(':', StringComparison.Ordinal),
            "Namespace e chave nao podem conter ':'.",
            "storage.key.invalid");

        return normalized;
    }
}