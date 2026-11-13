using GISORA.Domain.Common;
using GISORA.Domain.Geospatial;

namespace GISORA.Domain.Cemetery;

public sealed class Burial : Entity<Guid>
{
    internal Burial(Guid id, string personName, DateOnly buriedAt)
        : base(id)
    {
        PersonName = Guard.NotNullOrWhiteSpace(personName, nameof(personName)).Trim();
        BuriedAt = buriedAt;
    }

    public string PersonName { get; }

    public DateOnly BuriedAt { get; }

    public DateOnly? ExhumedAt { get; private set; }

    public bool IsActive => ExhumedAt is null;

    internal static Burial Rehydrate(Guid id, string personName, DateOnly buriedAt, DateOnly? exhumedAt)
        => new(id, personName, buriedAt) { ExhumedAt = exhumedAt };

    internal void Exhume(DateOnly exhumedAt)
    {
        Guard.Condition(IsActive, "Sepultamento ja encerrado.", "cemetery.burial.already-exhumed");
        Guard.Condition(exhumedAt >= BuriedAt, "Data de exumacao anterior ao sepultamento.", "cemetery.burial.exhumation-date");

        ExhumedAt = exhumedAt;
    }
}

public sealed class Grave : AggregateRoot<GraveId>
{
    private readonly List<Burial> _burials = [];

    private Grave(GraveId id, CemeteryId cemeteryId, CemeterySectorId sectorId, string code, Point location)
        : base(id)
    {
        CemeteryId = Guard.NotNull(cemeteryId, nameof(cemeteryId));
        SectorId = Guard.NotNull(sectorId, nameof(sectorId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code)).Trim();
        Location = Guard.NotNull(location, nameof(location));
    }

    public CemeteryId CemeteryId { get; }

    public CemeterySectorId SectorId { get; }

    public string Code { get; private set; }

    public Point Location { get; private set; }

    public IReadOnlyCollection<Burial> Burials => _burials.AsReadOnly();

    public Burial? ActiveBurial => _burials.FirstOrDefault(burial => burial.IsActive);

    public bool IsOccupied => ActiveBurial is not null;

    public static Grave Create(CemeteryId cemeteryId, CemeterySectorId sectorId, string code, Point location)
    {
        var grave = new Grave(GraveId.New(), cemeteryId, sectorId, code, location);
        grave.Raise(new GraveCreated(grave.Id, sectorId, grave.Code));

        return grave;
    }

    public Burial AddBurial(string personName, DateOnly buriedAt)
    {
        Guard.Condition(IsOccupied is false, "Jazigo ja possui sepultamento ativo.", "cemetery.grave.occupied");
        Guard.Condition(
            buriedAt <= DateOnly.FromDateTime(DateTime.UtcNow),
            "Data de sepultamento nao pode ser futura.",
            "cemetery.burial.future-date");

        var burial = new Burial(Guid.CreateVersion7(), personName, buriedAt);
        _burials.Add(burial);
        Raise(new BurialRegistered(Id, burial.Id, burial.PersonName));

        return burial;
    }

    public Burial Exhume(Guid burialId, DateOnly exhumedAt)
    {
        var burial = _burials.SingleOrDefault(candidate => candidate.Id == burialId)
                     ?? throw new DomainException("Sepultamento nao encontrado.", "cemetery.burial.not-found");

        burial.Exhume(exhumedAt);
        Raise(new BurialExhumed(Id, burial.Id));

        return burial;
    }

    public void Relocate(Point location) => Location = Guard.NotNull(location, nameof(location));

    internal static Grave Rehydrate(
        GraveId id,
        CemeteryId cemeteryId,
        CemeterySectorId sectorId,
        string code,
        Point location,
        IEnumerable<Burial> burials)
    {
        var grave = new Grave(id, cemeteryId, sectorId, code, location);
        grave._burials.AddRange(burials);

        return grave;
    }
}