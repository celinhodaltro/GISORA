using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;

namespace GISORA.Territory.Domain;

public sealed class UrbanBlock : AggregateRoot<UrbanBlockId>
{
    private readonly List<BlockFace> _faces = [];

    private UrbanBlock(UrbanBlockId id, FiscalSectorId fiscalSectorId, string code)
        : base(id)
    {
        FiscalSectorId = Guard.NotNull(fiscalSectorId, nameof(fiscalSectorId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim();
    }

    public FiscalSectorId FiscalSectorId { get; }

    public string Code { get; }

    public IReadOnlyCollection<BlockFace> Faces => _faces.AsReadOnly();

    public static UrbanBlock Create(FiscalSectorId fiscalSectorId, string code)
    {
        var urbanBlock = new UrbanBlock(UrbanBlockId.New(), fiscalSectorId, code);
        urbanBlock.Raise(new UrbanBlockCreated(urbanBlock.Id, fiscalSectorId, urbanBlock.Code));

        return urbanBlock;
    }

    public BlockFace AddFace(Guid streetId, LineString geometry, Distance frontage)
    {
        Guard.NotNull(geometry, nameof(geometry));

        Guard.Condition(
            _faces.All(face => face.StreetId != streetId),
            "Ja existe face para este logradouro no quarteirao.",
            "territory.blockface.duplicate-street");

        Guard.Condition(
            _faces.All(face => face.Geometry != geometry),
            "Ja existe face com geometria identica neste quarteirao.",
            "territory.blockface.duplicate-geometry");

        var blockFace = new BlockFace(Guid.CreateVersion7(), streetId, geometry, frontage);
        _faces.Add(blockFace);
        Raise(new BlockFaceAdded(Id, blockFace.Id));

        return blockFace;
    }

    internal static UrbanBlock Rehydrate(UrbanBlockId id, FiscalSectorId fiscalSectorId, string code, IEnumerable<BlockFace> faces)
    {
        var urbanBlock = new UrbanBlock(id, fiscalSectorId, code);
        urbanBlock._faces.AddRange(faces);

        return urbanBlock;
    }
}