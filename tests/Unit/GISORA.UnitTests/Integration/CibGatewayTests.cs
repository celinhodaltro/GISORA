using System.Net;
using System.Text;

using GISORA.Application.Integration;
using GISORA.Infrastructure.Integrations.Cib;

namespace GISORA.UnitTests.Integration;

public class CibGatewayTests
{
    private const string OfficialFixture = """
        {
          "InfoIbge": { "nomeMunicipio": "Belo Horizonte", "siglaUf": "MG", "codigoIbge": 3106200 },
          "Cib": { "valor": "CJNJJ50G", "situacao": "Ativa" },
          "DadosGeraisImovel": {
            "inscricaoImobiliaria": "1234567890123456789012310",
            "tipoImovel": 3, "tpArquitetonico": 11, "destinacaoImovel": 2,
            "areaTerreno": 350.75, "areaConstruida": 120.5, "bice": 1,
            "anoConstrutivo": 2005, "valorVenal": 450000, "dtUltimoValorVenal": "2024-12-01",
            "padraoConstrutivo": 3, "qtdGaragem": 2, "temPiscina": false,
            "valorRefMercado": 520000, "temBairro": true, "dataUltVlrMercado": "2024-12-15"
          },
          "AreaConstruidaCompl": { "areaPrivativa": 100.25, "areaComum": 20.25, "fraIdeal": 0.5 },
          "EnderecoImovel": {
            "tipoLogradouro": 100, "nomeLogradouro": "Avenida Brasil", "bairro": "Centro",
            "cep": "70040900", "numeroImovel": "1234", "complNroImovel": "BL A", "complEndereco": "APT 201"
          },
          "Titular": [
            {
              "niTitular": null, "nomeTitular": "JOAO DA SILVA", "percTitularidade": 1,
              "dtAquisicaoTitular": "2023-05-20", "docTitularidade": 1, "tipoTitularidade": 1,
              "nomeValido": true, "niTitularPrenchidoCorretamente": false, "dvniTitularValido": false
            }
          ],
          "ServicoRegistroImovel": {
            "nomeServentiaRI": "CARTORIO REGISTRO GERAL", "cnsRI": 123456, "cnmRI": null,
            "numMatriculaRI": "MAT123456789", "numUltimoAtoRI": "9876543",
            "lvCartRI": "LV01", "flCartRI": "FL02", "dtUltAtualizacao": "2024-11-30"
          },
          "CartorioNotas": {
            "nomeServentiaNotas": "CARTORIO NOTAS CENTRAL", "cnsNotas": 555555,
            "lvCartNotas": "LN01", "flCartNotas": "FL01"
          },
          "ITBI": {
            "baseCalculITBI": 500000, "dtTransacaoITBI": "2024-10-10", "tpTransacaoITBI": 1,
            "percTransacionadoITBI": 1, "valorRefITBI": 500000,
            "TransmitenteITBI": [
              { "nomeTransmitenteITBI": "MARIA PEREIRA", "idTransmitenteITBI": null, "dvvalidNi": false }
            ],
            "AdquirenteITBI": [
              { "nomeAdquirenteITBI": "JOAO DA SILVA", "idAdquirenteITBI": null, "percTransacAdquirenteITBI": 1 }
            ]
          }
        }
        """;

    [Fact]
    public async Task DemoGatewayServesDocumentedStructureWithoutCredentials()
    {
        var gateway = new DemoCibPropertyGateway();

        var result = await gateway.FindByCibAsync(" demo1a2b ");

        Assert.Equal(CibLookupStatus.Success, result.Status);
        Assert.True(result.IsDemonstration);
        Assert.NotNull(result.Record);
        Assert.Equal("DEMO1A2B", result.Record.Cib?.Valor);
        Assert.Equal(2610509, result.Record.InfoIbge?.CodigoIbge);
        Assert.Equal(350.75m, result.Record.DadosGeraisImovel?.AreaTerreno);
    }

    [Fact]
    public async Task DemoGatewayReportsUnknownCib()
    {
        var result = await new DemoCibPropertyGateway().FindByCibAsync("NAOEXIST");

        Assert.Equal(CibLookupStatus.NotFound, result.Status);
        Assert.True(result.IsDemonstration);
    }

    [Fact]
    public async Task ProxyGatewayParsesOfficialResponseAndNeverSendsToken()
    {
        var handler = new StubHandler(HttpStatusCode.OK, OfficialFixture);
        var gateway = new ProxyCibPropertyGateway(new HttpClient(handler) { BaseAddress = new Uri("https://proxy.local/") });

        var result = await gateway.FindByCibAsync("cjnjj50g");

        Assert.Equal(CibLookupStatus.Success, result.Status);
        Assert.False(result.IsDemonstration);
        Assert.Equal("CJNJJ50G", result.Record?.Cib?.Valor);
        Assert.Equal("Belo Horizonte", result.Record?.InfoIbge?.NomeMunicipio);
        Assert.Equal(350.75m, result.Record?.DadosGeraisImovel?.AreaTerreno);
        Assert.Equal(new DateOnly(2024, 12, 1), result.Record?.DadosGeraisImovel?.DtUltimoValorVenal);
        Assert.Single(result.Record!.Titular!);
        Assert.Equal("https://proxy.local/api/v1/ui/CJNJJ50G", handler.LastRequestUri?.ToString());
        Assert.Null(handler.LastAuthorization);
    }

    [Fact]
    public async Task ProxyGatewayMapsNotFound()
    {
        var gateway = new ProxyCibPropertyGateway(new HttpClient(new StubHandler(HttpStatusCode.NotFound, "{}"))
        {
            BaseAddress = new Uri("https://proxy.local/"),
        });

        var result = await gateway.FindByCibAsync("CJNJJ50G");

        Assert.Equal(CibLookupStatus.NotFound, result.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task ProxyGatewayMapsUnavailableStatuses(HttpStatusCode statusCode)
    {
        var gateway = new ProxyCibPropertyGateway(new HttpClient(new StubHandler(statusCode, "{}"))
        {
            BaseAddress = new Uri("https://proxy.local/"),
        });

        var result = await gateway.FindByCibAsync("CJNJJ50G");

        Assert.Equal(CibLookupStatus.Unavailable, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public async Task EmptyCibIsRejectedLocally()
    {
        var gateway = new ProxyCibPropertyGateway(new HttpClient(new StubHandler(HttpStatusCode.OK, "{}"))
        {
            BaseAddress = new Uri("https://proxy.local/"),
        });

        var result = await gateway.FindByCibAsync("   ");

        Assert.Equal(CibLookupStatus.Invalid, result.Status);
    }

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        public string? LastAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastAuthorization = request.Headers.Authorization?.ToString();

            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }
}