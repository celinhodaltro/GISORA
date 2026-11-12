using GISORA.Territory.Domain;

namespace GISORA.Territory.Application;

public sealed record CoordinateDto(double X, double Y);

public sealed record BlockFaceDto(
    Guid Id,
    Guid StreetId,
    IReadOnlyList<CoordinateDto> Coordinates,
    decimal FrontageMeters);

public sealed record UrbanBlockDto(
    UrbanBlockId Id,
    FiscalSectorId FiscalSectorId,
    string Code,
    IReadOnlyList<BlockFaceDto> Faces);

public sealed record UrbanBlockListItem(
    UrbanBlockId Id,
    FiscalSectorId FiscalSectorId,
    string Code,
    int FaceCount);

internal static class UrbanBlockMapper
{
    public static UrbanBlockDto ToDto(this UrbanBlock urbanBlock)
        => new(
            urbanBlock.Id,
            urbanBlock.FiscalSectorId,
            urbanBlock.Code,
            urbanBlock.Faces
                .Select(face => new BlockFaceDto(
                    face.Id,
                    face.StreetId,
                    face.Geometry.Coordinates.Select(coordinate => new CoordinateDto(coordinate.X, coordinate.Y)).ToArray(),
                    face.Frontage.Meters))
                .ToArray());

    public static UrbanBlockListItem ToListItem(this UrbanBlock urbanBlock)
        => new(urbanBlock.Id, urbanBlock.FiscalSectorId, urbanBlock.Code, urbanBlock.Faces.Count);
}