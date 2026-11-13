using GISORA.Integration.Application;

namespace GISORA.Infrastructure.Integration.Cib;

public sealed class DemoCibPropertyGateway : ICibPropertyGateway
{
    private static readonly IReadOnlyDictionary<string, CibPropertyRecord> Catalog =
        new Dictionary<string, CibPropertyRecord>(StringComparer.OrdinalIgnoreCase)
        {
            ["DEMO1A2B"] = Record(
                cib: "DEMO1A2B",
                situacao: "Ativa",
                inscricao: "0000000000000000000000001",
                areaTerreno: 350.75m,
                areaConstruida: 120.5m,
                anoConstrutivo: 2005,
                tipoImovel: 3,
                tpArquitetonico: 11,
                destinacaoImovel: 2,
                padraoConstrutivo: 3,
                qtdGaragem: 2,
                temPiscina: false,
                valorVenal: 450000m,
                nomeLogradouro: "Rua da Matriz",
                bairro: "Centro",
                numero: "10",
                cep: "55685000",
                titular: "MARIA DE LOURDES EXAMPLE",
                areaPrivativa: 100.25m,
                areaComum: 20.25m),
            ["DEMO3C4D"] = Record(
                cib: "DEMO3C4D",
                situacao: "Ativa",
                inscricao: "0000000000000000000000002",
                areaTerreno: 420m,
                areaConstruida: 96.4m,
                anoConstrutivo: 2012,
                tipoImovel: 3,
                tpArquitetonico: 12,
                destinacaoImovel: 2,
                padraoConstrutivo: 2,
                qtdGaragem: 1,
                temPiscina: false,
                valorVenal: 320000m,
                nomeLogradouro: "Avenida Central",
                bairro: "Centro",
                numero: "120",
                cep: "55685000",
                titular: "JOAO EXAMPLE DE SOUZA",
                areaPrivativa: 78.1m,
                areaComum: 18.3m),
            ["DEMO5E6F"] = Record(
                cib: "DEMO5E6F",
                situacao: "Extinta",
                inscricao: "0000000000000000000000003",
                areaTerreno: 360m,
                areaConstruida: 74m,
                anoConstrutivo: 1998,
                tipoImovel: 1,
                tpArquitetonico: 1,
                destinacaoImovel: 1,
                padraoConstrutivo: 1,
                qtdGaragem: 0,
                temPiscina: false,
                valorVenal: 150000m,
                nomeLogradouro: "Travessa do Mercado",
                bairro: "Sao Jose",
                numero: "45",
                cep: "55685000",
                titular: "ANTONIO EXAMPLE DA SILVA",
                areaPrivativa: 74m,
                areaComum: 0m),
        };

    public bool IsDemonstration => true;

    public static IReadOnlyList<string> DemoCibs => Catalog.Keys.ToArray();

    public Task<CibLookupResult> FindByCibAsync(string cib, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(cib);

        if (normalized.Length == 0)
        {
            return Task.FromResult(CibLookupResult.Invalid("Informe o CIB para consulta."));
        }

        return Task.FromResult(Catalog.TryGetValue(normalized, out var record)
            ? CibLookupResult.Found(record, isDemonstration: true)
            : CibLookupResult.NotFound(isDemonstration: true));
    }

    internal static string Normalize(string? cib) => (cib ?? string.Empty).Trim().ToUpperInvariant();

    private static CibPropertyRecord Record(
        string cib,
        string situacao,
        string inscricao,
        decimal areaTerreno,
        decimal areaConstruida,
        int anoConstrutivo,
        int tipoImovel,
        int tpArquitetonico,
        int destinacaoImovel,
        int padraoConstrutivo,
        int qtdGaragem,
        bool temPiscina,
        decimal valorVenal,
        string nomeLogradouro,
        string bairro,
        string numero,
        string cep,
        string titular,
        decimal areaPrivativa,
        decimal areaComum)
        => new(
            new CibInfoIbge("Passira", "PE", 2610509),
            new CibValue(cib, situacao),
            new CibDadosGeraisImovel(
                inscricao,
                tipoImovel,
                tpArquitetonico,
                destinacaoImovel,
                areaTerreno,
                areaConstruida,
                1,
                anoConstrutivo,
                valorVenal,
                new DateOnly(2024, 12, 1),
                padraoConstrutivo,
                qtdGaragem,
                temPiscina,
                null,
                true,
                null),
            new CibAreaConstruidaCompl(areaPrivativa, areaComum, null),
            new CibEnderecoImovel(100, nomeLogradouro, bairro, cep, numero, null, null),
            [
                new CibTitular(null, titular, 1m, new DateOnly(2023, 5, 20), 1, 1, true, false, false),
            ],
            new CibServicoRegistroImovel(
                "CARTORIO DE REGISTRO DE IMOVEIS DE PASSIRA",
                123456,
                null,
                "MAT000000001",
                "ATO000001",
                "LV01",
                "FL02",
                new DateOnly(2024, 11, 30)),
            new CibCartorioNotas("CARTORIO DE NOTAS DE PASSIRA", 555555, "LN01", "FL01"),
            null);
}