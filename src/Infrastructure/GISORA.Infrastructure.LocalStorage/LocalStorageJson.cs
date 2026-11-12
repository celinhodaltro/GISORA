using System.Text.Json;
using System.Text.Json.Serialization;

namespace GISORA.Infrastructure.LocalStorage;

internal static class LocalStorageJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}