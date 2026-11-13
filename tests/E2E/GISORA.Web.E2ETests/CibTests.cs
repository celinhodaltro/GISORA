using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class CibTests(WebAppFixture fixture)
{
    [Fact]
    public async Task QueriesDemoCibAndRendersPanel()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cib");

        await Expect(page.Locator(".cib-mode__badge")).ToContainTextAsync("Modo demonstracao");

        await page.GetByPlaceholder("Codigo CIB").FillAsync("DEMO1A2B");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Consultar CIB" }).ClickAsync();

        await Expect(page.Locator(".cib-panel__cib")).ToHaveTextAsync("DEMO1A2B");
        await Expect(page.Locator(".cib-panel__notice")).ToContainTextAsync("Dados simulados");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("Passira");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("PE");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("Caracteristicas territoriais");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("Caracteristicas construtivas");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("Informacoes cadastrais");
        await Expect(page.Locator(".cib-panel")).ToContainTextAsync("Titularidade");
    }

    [Fact]
    public async Task ReportsUnknownCib()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cib");

        await page.GetByPlaceholder("Codigo CIB").FillAsync("NAOEXIST");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Consultar CIB" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Nenhuma unidade imobiliaria encontrada");
    }

    [Fact]
    public async Task SelectingPropertyOnMapOpensItsPanel()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await page.GotoAsync($"{fixture.BaseUrl}/cib");
        await page.WaitForTimeoutAsync(500);

        var selected = false;

        foreach (var relativeY in new[] { 0.2, 0.5, 0.8 })
        {
            foreach (var relativeX in new[] { 0.15, 0.5, 0.85 })
            {
                await ClickMapAsync(page, relativeX, relativeY);

                if (await page.Locator(".cib-section--selected").CountAsync() > 0)
                {
                    selected = true;
                    break;
                }
            }

            if (selected)
            {
                break;
            }
        }

        Assert.True(selected, "A selecao de imovel no mapa nao abriu o painel lateral.");
        await Expect(page.Locator(".cib-section--selected")).ToContainTextAsync("Inscricao municipal");
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