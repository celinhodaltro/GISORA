using GISORA.Domain.Geospatial;
using GISORA.Domain.PublicFacilities;
using GISORA.Domain.Rural;

namespace GISORA.Presentation.Shared.Maps;

public enum CadastroModuleKind
{
    Quadras,
    Imoveis,
    Equipamentos,
    Rurais,
}

public sealed record CadastroOption(Guid Id, string Label);

public sealed record CadastroOptions(
    IReadOnlyList<CadastroOption> Sectors,
    IReadOnlyList<CadastroOption> Streets,
    IReadOnlyList<CadastroOption> Neighborhoods,
    IReadOnlyList<FacilitySymbol> FacilityTypes);

public sealed record CadastroDetailRow(string Label, string Value);

public sealed class CadastroForm
{
    public bool IsNew { get; set; }

    public string? EntityId { get; set; }

    public Geometry? Geometry { get; set; }

    public bool GeometryChanged { get; set; }

    public string? AreaLabel { get; set; }

    public Guid? SectorId { get; set; }

    public string? Code { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? Lot { get; set; }

    public string? Unit { get; set; }

    public Guid? StreetId { get; set; }

    public string? Number { get; set; }

    public Guid? NeighborhoodId { get; set; }

    public decimal LandArea { get; set; }

    public string? Registration { get; set; }

    public string? Name { get; set; }

    public FacilityType Type { get; set; }

    public Guid? FacilityNeighborhoodId { get; set; }

    public string? RuralName { get; set; }

    public RuralUsage Usage { get; set; }

    public string? CarCode { get; set; }

    public string? SigefCode { get; set; }

    public string? SnciCode { get; set; }

    public IReadOnlyList<CadastroDetailRow> Details { get; set; } = [];
}

public sealed record CadastroSelection(
    CadastroModuleKind Module,
    string LayerId,
    string FeatureId,
    IReadOnlyDictionary<string, string> Properties,
    bool IsDraft);