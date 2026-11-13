
namespace GISORA.Integration.Application;

public sealed record CibInfoIbge(
    string? NomeMunicipio,
    string? SiglaUf,
    int? CodigoIbge);

public sealed record CibValue(
    string? Valor,
    string? Situacao);

public sealed record CibDadosGeraisImovel(
    string? InscricaoImobiliaria,
    int? TipoImovel,
    int? TpArquitetonico,
    int? DestinacaoImovel,
    decimal? AreaTerreno,
    decimal? AreaConstruida,
    int? Bice,
    int? AnoConstrutivo,
    decimal? ValorVenal,
    DateOnly? DtUltimoValorVenal,
    int? PadraoConstrutivo,
    int? QtdGaragem,
    bool? TemPiscina,
    decimal? ValorRefMercado,
    bool? TemBairro,
    DateOnly? DataUltVlrMercado);

public sealed record CibAreaConstruidaCompl(
    decimal? AreaPrivativa,
    decimal? AreaComum,
    decimal? FraIdeal);

public sealed record CibEnderecoImovel(
    int? TipoLogradouro,
    string? NomeLogradouro,
    string? Bairro,
    string? Cep,
    string? NumeroImovel,
    string? ComplNroImovel,
    string? ComplEndereco);

public sealed record CibTitular(
    string? NiTitular,
    string? NomeTitular,
    decimal? PercTitularidade,
    DateOnly? DtAquisicaoTitular,
    int? DocTitularidade,
    int? TipoTitularidade,
    bool? NomeValido,
    bool? NiTitularPrenchidoCorretamente,
    bool? DvniTitularValido);

public sealed record CibServicoRegistroImovel(
    string? NomeServentiaRI,
    int? CnsRI,
    string? CnmRI,
    string? NumMatriculaRI,
    string? NumUltimoAtoRI,
    string? LvCartRI,
    string? FlCartRI,
    DateOnly? DtUltAtualizacao);

public sealed record CibCartorioNotas(
    string? NomeServentiaNotas,
    int? CnsNotas,
    string? LvCartNotas,
    string? FlCartNotas);

public sealed record CibTransmitente(
    string? NomeTransmitenteITBI,
    string? IdTransmitenteITBI,
    bool? DvvalidNi,
    bool? IdTransmitentePreenchidoCorretamente,
    bool? NomeTransmitenteValido);

public sealed record CibAdquirente(
    string? NomeAdquirenteITBI,
    string? IdAdquirenteITBI,
    decimal? PercTransacAdquirenteITBI,
    bool? PercTransacAdquirenteITBIValido,
    bool? NomeAdquirenteValido,
    bool? IdAdquirentePreenchidoCorretamente,
    bool? DvvalidNi);

public sealed record CibItbi(
    decimal? BaseCalculITBI,
    DateOnly? DtTransacaoITBI,
    int? TpTransacaoITBI,
    decimal? PercTransacionadoITBI,
    decimal? ValorRefITBI,
    IReadOnlyList<CibTransmitente>? TransmitenteITBI,
    IReadOnlyList<CibAdquirente>? AdquirenteITBI);

public sealed record CibPropertyRecord(
    CibInfoIbge? InfoIbge,
    CibValue? Cib,
    CibDadosGeraisImovel? DadosGeraisImovel,
    CibAreaConstruidaCompl? AreaConstruidaCompl,
    CibEnderecoImovel? EnderecoImovel,
    IReadOnlyList<CibTitular>? Titular,
    CibServicoRegistroImovel? ServicoRegistroImovel,
    CibCartorioNotas? CartorioNotas,
    CibItbi? Itbi);

public enum CibLookupStatus
{
    Success,
    NotFound,
    Invalid,
    Unavailable,
}

public sealed record CibLookupResult(
    CibLookupStatus Status,
    CibPropertyRecord? Record,
    bool IsDemonstration,
    string? Error)
{
    public static CibLookupResult Found(CibPropertyRecord record, bool isDemonstration)
        => new(CibLookupStatus.Success, record, isDemonstration, null);

    public static CibLookupResult NotFound(bool isDemonstration)
        => new(CibLookupStatus.NotFound, null, isDemonstration, "Nenhuma unidade imobiliaria encontrada para o CIB informado.");

    public static CibLookupResult Invalid(string message)
        => new(CibLookupStatus.Invalid, null, false, message);

    public static CibLookupResult Unavailable(bool isDemonstration, string message)
        => new(CibLookupStatus.Unavailable, null, isDemonstration, message);
}

public interface ICibPropertyGateway
{
    bool IsDemonstration { get; }

    Task<CibLookupResult> FindByCibAsync(string cib, CancellationToken cancellationToken = default);
}