using System.Net.Http.Json;
using System.Text.Json.Serialization;

using GISORA.Infrastructure.LocalStorage;
using GISORA.Territory.Application;

namespace GISORA.Infrastructure.Integration;

public sealed class IbgeMunicipalityCatalog(HttpClient httpClient, ILocalStore store) : IMunicipalityCatalog
{
    private const string StorageNamespace = "ibge";
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromDays(7);

    private static readonly StateOption[] States =
    [
        new("AC", "Acre"),
        new("AL", "Alagoas"),
        new("AP", "Amapa"),
        new("AM", "Amazonas"),
        new("BA", "Bahia"),
        new("CE", "Ceara"),
        new("DF", "Distrito Federal"),
        new("ES", "Espirito Santo"),
        new("GO", "Goias"),
        new("MA", "Maranhao"),
        new("MT", "Mato Grosso"),
        new("MS", "Mato Grosso do Sul"),
        new("MG", "Minas Gerais"),
        new("PA", "Para"),
        new("PB", "Paraiba"),
        new("PR", "Parana"),
        new("PE", "Pernambuco"),
        new("PI", "Piaui"),
        new("RJ", "Rio de Janeiro"),
        new("RN", "Rio Grande do Norte"),
        new("RS", "Rio Grande do Sul"),
        new("RO", "Rondonia"),
        new("RR", "Roraima"),
        new("SC", "Santa Catarina"),
        new("SP", "Sao Paulo"),
        new("SE", "Sergipe"),
        new("TO", "Tocantins"),
    ];

    public Task<IReadOnlyList<StateOption>> GetStatesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StateOption>>(States);

    public async Task<IReadOnlyList<MunicipalityOption>> GetMunicipalitiesAsync(
        string stateAbbreviation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateAbbreviation);

        var state = stateAbbreviation.Trim().ToUpperInvariant();
        var cached = await ReadCacheAsync(state, cancellationToken);

        if (cached is not null && DateTimeOffset.UtcNow - cached.CachedAtUtc < CacheLifetime)
        {
            return cached.Items;
        }

        try
        {
            var response = await httpClient.GetAsync(
                $"api/v1/localidades/estados/{state}/municipios?orderBy=nome",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<IbgeMunicipality[]>(cancellationToken)
                          ?? [];

            var items = payload
                .Select(municipality => new MunicipalityOption(municipality.Id, municipality.Nome, state))
                .ToArray();

            await WriteCacheAsync(state, items, cancellationToken);

            return items;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or NotSupportedException)
        {
            if (cached is not null)
            {
                return cached.Items;
            }

            throw new MunicipalityCatalogException(
                "Nao foi possivel consultar o catalogo de municipios do IBGE. Verifique a conexao.",
                exception);
        }
    }

    private async Task<MunicipalityCache?> ReadCacheAsync(string state, CancellationToken cancellationToken)
        => await store.GetAsync<MunicipalityCache>(StorageNamespace, $"municipalities-{state}", cancellationToken);

    private Task WriteCacheAsync(string state, IReadOnlyList<MunicipalityOption> items, CancellationToken cancellationToken)
        => store.SetAsync(
            StorageNamespace,
            $"municipalities-{state}",
            new MunicipalityCache(DateTimeOffset.UtcNow, items),
            cancellationToken);

    private sealed record MunicipalityCache(
        [property: JsonPropertyName("cachedAtUtc")] DateTimeOffset CachedAtUtc,
        [property: JsonPropertyName("items")] IReadOnlyList<MunicipalityOption> Items);

    private sealed record IbgeMunicipality(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("nome")] string Nome);
}