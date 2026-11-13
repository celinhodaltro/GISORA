using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class PlanningTests(WebAppFixture fixture)
{
    [Fact]
    public async Task FindsZoneIncidentToSelectedProperty()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await page.GotoAsync($"{fixture.BaseUrl}/planning");

        await page.GetByPlaceholder("Codigo da zona").FillAsync("ZR1");
        await page.GetByPlaceholder("Nome da zona").FillAsync("Zona residencial");
        await page.GetByPlaceholder("Taxa de ocupacao (%)").FillAsync("60");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar zona" }).ClickAsync();

        await ClickMapAsync(page, 0.15, 0.15);
        await ClickMapAsync(page, 0.85, 0.15);
        await ClickMapAsync(page, 0.85, 0.85);
        await ClickMapAsync(page, 0.15, 0.85);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir zona" }).ClickAsync();

        await Expect(page.Locator(".planning-area")).ToContainTextAsync("ha");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar zona" }).ClickAsync();
        await Expect(page.GetByText("Zona cadastrada.")).ToBeVisibleAsync();

        await page.Locator("select[aria-label='Imovel']").SelectOptionAsync(new SelectOptionValue { Index = 0 });
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Consultar incidencia" }).ClickAsync();

        await Expect(page.Locator(".planning-zone")).ToHaveCountAsync(1);
        await Expect(page.Locator(".planning-zone")).ToContainTextAsync("ZR1");
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