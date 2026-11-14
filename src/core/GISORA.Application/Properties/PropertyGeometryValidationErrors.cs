using GISORA.Application.Common;
using GISORA.Application.Geospatial;

namespace GISORA.Application.Properties;

internal static class PropertyGeometryValidationErrors
{
    public static Error From(GeometryValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);

        var overlap = validation.Issues.FirstOrDefault(issue => issue.Code == PropertyGeometryValidator.OverlapCode);

        if (overlap is not null)
        {
            return Error.Conflict(overlap.Message, overlap.Code);
        }

        return Error.Validation(
            $"Geometria invalida: {string.Join(" ", validation.Errors)}",
            "properties.geometry.invalid");
    }
}