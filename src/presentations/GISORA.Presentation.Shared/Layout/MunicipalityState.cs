using GISORA.Application.Territory;

namespace GISORA.Presentation.Shared.Layout;

public sealed class MunicipalityState(ITerritoryQueries queries)
{
    public string Name { get; private set; } = "Nao definido";

    public event Action? Changed;

    public async Task RefreshAsync()
    {
        try
        {
            var municipalities = await queries.ListMunicipalitiesAsync();
            Name = municipalities.Count > 0 ? municipalities[0].Name : "Nao definido";
        }
        catch
        {
            Name = "Nao definido";
        }

        Changed?.Invoke();
    }
}