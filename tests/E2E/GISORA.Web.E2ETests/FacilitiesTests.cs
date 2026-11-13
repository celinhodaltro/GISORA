using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class FacilitiesTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesFacilityWithPickedLocationAndKeepsItAfterReload()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/facilities");

        await page.GetByPlaceholder("Nome do equipamento").FillAsync("Escola Municipal");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Marcar no mapa" }).ClickAsync();
        await ClickMapAsync(page, 0.5, 0.5);

        await Expect(page.Locator(".facilities__coordinate")).ToContainTextAsync(",");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar equipamento" }).ClickAsync();

        await Expect(page.GetByText("Equipamento cadastrado.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Escola Municipal", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(page.Locator(".leaflet-marker-icon")).ToHaveCountAsync(1);

        await page.ReloadAsync();

        await Expect(page.GetByText("Escola Municipal", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task ShowsDistinctSymbolsAndLegendPerFacilityType()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/facilities");

        await CreateAtAsync(page, "Escola Municipal", "School", 0.4, 0.45);
        await CreateAtAsync(page, "Parque Municipal", "Park", 0.6, 0.55);

        await Expect(page.Locator(".gisora-map-pin--school")).ToHaveCountAsync(1);
        await Expect(page.Locator(".gisora-map-pin--park")).ToHaveCountAsync(1);

        var legend = page.Locator(".gisora-map-legend");
        await Expect(legend).ToContainTextAsync("Escola");
        await Expect(legend).ToContainTextAsync("Parque");
        await Expect(legend).Not.ToContainTextAsync("Hospital");
    }

    [Fact]
    public async Task RejectsDuplicatedFacilityName()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/facilities");

        await CreateAsync(page, "Praca Central");
        await Expect(page.GetByText("Praca Central", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await CreateAsync(page, "Praca Central");

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Ja existe equipamento com este nome no bairro.");
    }

    private static async Task CreateAsync(IPage page, string name)
    {
        await page.GetByPlaceholder("Nome do equipamento").FillAsync(name);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar equipamento" }).ClickAsync();
    }

    private static async Task CreateAtAsync(IPage page, string name, string type, double relativeX, double relativeY)
    {
        await page.GetByPlaceholder("Nome do equipamento").FillAsync(name);
        await page.Locator(".facilities-form__select").First.SelectOptionAsync(type);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Marcar no mapa" }).ClickAsync();
        await ClickMapAsync(page, relativeX, relativeY);
        await Expect(page.Locator(".facilities__coordinate")).ToContainTextAsync(",");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar equipamento" }).ClickAsync();
        await Expect(page.GetByText("Equipamento cadastrado.")).ToBeVisibleAsync();
    }

    private static async Task ClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var map = page.Locator(".gisora-map");
        await map.ScrollIntoViewIfNeededAsync();
        await page.WaitForTimeoutAsync(250);

        var box = await map.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
    }
}