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

        var box = await page.Locator(".gisora-map").BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);
        await page.Mouse.ClickAsync(box.X + (box.Width * 0.5f), box.Y + (box.Height * 0.5f));

        await Expect(page.Locator(".facilities__coordinate")).ToContainTextAsync(",");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar equipamento" }).ClickAsync();

        await Expect(page.GetByText("Equipamento cadastrado.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Escola Municipal", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(page.Locator(".leaflet-marker-icon")).ToHaveCountAsync(1);

        await page.ReloadAsync();

        await Expect(page.GetByText("Escola Municipal", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
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
}