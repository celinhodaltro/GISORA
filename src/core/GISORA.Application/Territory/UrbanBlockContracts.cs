using GISORA.Domain.Territory;

namespace GISORA.Application.Territory;

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
    IReadOnlyList<BlockFaceDto> Faces,
    IReadOnlyList<IReadOnlyList<CoordinateDto>> Rings);

public sealed record UrbanBlockListItem(
    UrbanBlockId Id,
    FiscalSectorId FiscalSectorId,
    string Code,
    int FaceCount,
    bool HasGeometry);

public sealed record UrbanBlockMapItem(
    UrbanBlockId Id,
    string Code,
    IReadOnlyList<IReadOnlyList<CoordinateDto>> Rings,
    IReadOnlyList<BlockFaceDto> Faces)
{
    public static UrbanBlockMapItem From(UrbanBlock urbanBlock)
        => new(
            urbanBlock.Id,
            urbanBlock.Code,
            urbanBlock.Geometry is null
                ? []
                : urbanBlock.Geometry.Rings
                    .Select(ring => (IReadOnlyList<CoordinateDto>)ring.Coordinates
                        .Select(coordinate => new CoordinateDto(coordinate.X, coordinate.Y))
                        .ToArray())
                    .ToArray(),
            urbanBlock.Faces
                .Select(face => new BlockFaceDto(
                    face.Id,
                    face.StreetId,
                    face.Geometry.Coordinates.Select(coordinate => new CoordinateDto(coordinate.X, coordinate.Y)).ToArray(),
                    face.Frontage.Meters))
                .ToArray());
}

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
                .ToArray(),
            ToRings(urbanBlock));

    public static UrbanBlockListItem ToListItem(this UrbanBlock urbanBlock)
        => new(urbanBlock.Id, urbanBlock.FiscalSectorId, urbanBlock.Code, urbanBlock.Faces.Count, urbanBlock.Geometry is not null);

    private static IReadOnlyList<CoordinateDto>[] ToRings(UrbanBlock urbanBlock)
        => urbanBlock.Geometry is null
            ? []
            : urbanBlock.Geometry.Rings
                .Select(ring => (IReadOnlyList<CoordinateDto>)ring.Coordinates
                    .Select(coordinate => new CoordinateDto(coordinate.X, coordinate.Y))
                    .ToArray())
                .ToArray();
}