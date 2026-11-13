using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using GISORA.Application.Integration;

namespace GISORA.Infrastructure.Integrations.Cib;

public sealed class ProxyCibPropertyGateway(HttpClient httpClient) : ICibPropertyGateway
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public bool IsDemonstration => false;

    public async Task<CibLookupResult> FindByCibAsync(string cib, CancellationToken cancellationToken = default)
    {
        var normalized = DemoCibPropertyGateway.Normalize(cib);

        if (normalized.Length == 0)
        {
            return CibLookupResult.Invalid("Informe o CIB para consulta.");
        }

        try
        {
            using var response = await httpClient.GetAsync(
                $"api/v1/ui/{Uri.EscapeDataString(normalized)}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return CibLookupResult.NotFound(isDemonstration: false);
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return CibLookupResult.Unavailable(
                    isDemonstration: false,
                    "A consulta ao SINTER nao foi autorizada. Verifique a credencial do servico de integracao.");
            }

            if (response.IsSuccessStatusCode is false)
            {
                return CibLookupResult.Unavailable(
                    isDemonstration: false,
                    $"SINTER indisponivel no momento (HTTP {(int)response.StatusCode}).");
            }

            var record = await response.Content.ReadFromJsonAsync<CibPropertyRecord>(SerializerOptions, cancellationToken);

            return record is null
                ? CibLookupResult.Unavailable(isDemonstration: false, "Resposta vazia do SINTER.")
                : CibLookupResult.Found(record, isDemonstration: false);
        }
        catch (JsonException)
        {
            return CibLookupResult.Unavailable(
                isDemonstration: false,
                "Resposta do SINTER em formato inesperado.");
        }
        catch (HttpRequestException)
        {
            return CibLookupResult.Unavailable(
                isDemonstration: false,
                "Nao foi possivel alcancar o servico de integracao do SINTER.");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested is false)
        {
            return CibLookupResult.Unavailable(
                isDemonstration: false,
                "A consulta ao SINTER excedeu o tempo limite.");
        }
    }
}

public sealed class UnavailableCibPropertyGateway(string reason) : ICibPropertyGateway
{
    public bool IsDemonstration => false;

    public Task<CibLookupResult> FindByCibAsync(string cib, CancellationToken cancellationToken = default)
        => Task.FromResult(CibLookupResult.Unavailable(isDemonstration: false, reason));
}