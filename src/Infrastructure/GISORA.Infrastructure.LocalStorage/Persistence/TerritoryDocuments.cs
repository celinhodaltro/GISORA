using GISORA.Geospatial.Domain;
using GISORA.Territory.Domain;

namespace GISORA.Infrastructure.LocalStorage.Persistence;

internal sealed class MunicipalityDocument
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string IbgeCodeValue { get; init; } = string.Empty;

    public GeometryDocument? Boundary { get; init; }

    public Municipality ToDomain() => Municipality.Rehydrate(
        new MunicipalityId(Id),
        Name,
        GISORA.Territory.Domain.IbgeCode.Parse(IbgeCodeValue),
        Boundary?.ToDomain());

    public static MunicipalityDocument FromDomain(Municipality municipality) => new()
    {
        Id = municipality.Id.Value,
        Name = municipality.Name,
        IbgeCodeValue = municipality.IbgeCode.Value,
        Boundary = municipality.Boundary is null ? null : GeometryDocument.FromDomain(municipality.Boundary),
    };
}

internal sealed class DistrictDocument
{
    public Guid Id { get; init; }

    public Guid MunicipalityId { get; init; }

    public string Name { get; init; } = string.Empty;

    public District ToDomain() => District.Rehydrate(new DistrictId(Id), new MunicipalityId(MunicipalityId), Name);

    public static DistrictDocument FromDomain(District district) => new()
    {
        Id = district.Id.Value,
        MunicipalityId = district.MunicipalityId.Value,
        Name = district.Name,
    };
}

internal sealed class NeighborhoodDocument
{
    public Guid Id { get; init; }

    public Guid DistrictId { get; init; }

    public string Name { get; init; } = string.Empty;

    public Neighborhood ToDomain() => Neighborhood.Rehydrate(new NeighborhoodId(Id), new DistrictId(DistrictId), Name);

    public static NeighborhoodDocument FromDomain(Neighborhood neighborhood) => new()
    {
        Id = neighborhood.Id.Value,
        DistrictId = neighborhood.DistrictId.Value,
        Name = neighborhood.Name,
    };
}

internal sealed class FiscalSectorDocument
{
    public Guid Id { get; init; }

    public Guid DistrictId { get; init; }

    public string Code { get; init; } = string.Empty;

    public FiscalSector ToDomain() => FiscalSector.Rehydrate(new FiscalSectorId(Id), new DistrictId(DistrictId), Code);

    public static FiscalSectorDocument FromDomain(FiscalSector sector) => new()
    {
        Id = sector.Id.Value,
        DistrictId = sector.DistrictId.Value,
        Code = sector.Code,
    };
}

internal sealed class BlockFaceDocument
{
    public Guid Id { get; init; }

    public Guid StreetId { get; init; }

    public GeometryDocument Geometry { get; init; } = null!;

    public decimal FrontageMeters { get; init; }

    public BlockFace ToDomain() => new(Id, StreetId, (LineString)Geometry.ToDomain(), Distance.FromMeters(FrontageMeters));

    public static BlockFaceDocument FromDomain(BlockFace face) => new()
    {
        Id = face.Id,
        StreetId = face.StreetId,
        Geometry = GeometryDocument.FromDomain(face.Geometry),
        FrontageMeters = face.Frontage.Meters,
    };
}

internal sealed class UrbanBlockDocument
{
    public Guid Id { get; init; }

    public Guid FiscalSectorId { get; init; }

    public string Code { get; init; } = string.Empty;

    public List<BlockFaceDocument> Faces { get; init; } = [];

    public UrbanBlock ToDomain() => UrbanBlock.Rehydrate(
        new UrbanBlockId(Id),
        new FiscalSectorId(FiscalSectorId),
        Code,
        Faces.Select(face => face.ToDomain()));

    public static UrbanBlockDocument FromDomain(UrbanBlock block) => new()
    {
        Id = block.Id.Value,
        FiscalSectorId = block.FiscalSectorId.Value,
        Code = block.Code,
        Faces = block.Faces.Select(BlockFaceDocument.FromDomain).ToList(),
    };
}

internal sealed record UrbanBlockIndexEntry
{
    public Guid Id { get; init; }

    public Guid FiscalSectorId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string CodeSearch { get; init; } = string.Empty;

    public int FaceCount { get; init; }

    public static UrbanBlockIndexEntry FromDomain(UrbanBlock block) => new()
    {
        Id = block.Id.Value,
        FiscalSectorId = block.FiscalSectorId.Value,
        Code = block.Code,
        CodeSearch = block.Code.ToUpperInvariant(),
        FaceCount = block.Faces.Count,
    };
}