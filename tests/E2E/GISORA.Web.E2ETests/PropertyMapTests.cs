using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class PropertyMapTests(WebAppFixture fixture)
{
    private const string PropertyId = "0193f3a4-0000-7000-8000-000000000001";
    private const string StreetId = "0193f3a4-0000-7000-8000-000000000002";
    private const string NeighborhoodId = "0193f3a4-0000-7000-8000-000000000003";

    [Fact]
    public async Task MapShowsEmptyStateWithoutGeometries()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/map");

        await Expect(page.GetByText("Nenhum imovel com geometria")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MapRendersPropertyAndSelectionShowsAttributes()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/map");

        await Expect(page.Locator("path.leaflet-interactive")).ToHaveCountAsync(1);

        await page.EvaluateAsync("""
            () => document.querySelector('path.leaflet-interactive')
                .dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }))
            """);

        await Expect(page.GetByText("001.002.003")).ToBeVisibleAsync();
        await Expect(page.GetByText("500.00")).ToBeVisibleAsync();
    }

    private static async Task SeedAsync(IPage page)
    {
        var document = $$"""
            {
              "id": "{{PropertyId}}",
              "sector": "001",
              "block": "002",
              "lot": "003",
              "unit": null,
              "streetId": "{{StreetId}}",
              "number": "10",
              "neighborhoodId": "{{NeighborhoodId}}",
              "complement": null,
              "landAreaSquareMeters": 500,
              "geometry": {
                "type": "Polygon",
                "authority": "EPSG",
                "sridCode": 4326,
                "rings": [[
                  { "x": -35.25, "y": -7.95, "z": null },
                  { "x": -35.24, "y": -7.95, "z": null },
                  { "x": -35.24, "y": -7.94, "z": null },
                  { "x": -35.25, "y": -7.94, "z": null },
                  { "x": -35.25, "y": -7.95, "z": null }
                ]],
                "parts": []
              },
              "buildings": [],
              "units": [],
              "ownerships": [],
              "history": []
            }
            """;

        await page.EvaluateAsync("""
            (data) => {
              localStorage.setItem(data.documentKey, data.document);
              localStorage.setItem('gisora:properties:property-ids', JSON.stringify([data.id]));
            }
            """, new
        {
            id = PropertyId,
            documentKey = $"gisora:properties:property-{PropertyId}",
            document,
        });
    }
}