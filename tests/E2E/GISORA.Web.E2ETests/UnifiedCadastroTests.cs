using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class UnifiedCadastroTests(WebAppFixture fixture)
{
    [Fact]
    public async Task LoadsAllLayersAndSwitchesModulesFromIconBar()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro");

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();
        await Expect(page.Locator(".cadastro-bar__item")).ToHaveCountAsync(4);

        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveCountAsync(1);
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Quadras");

        var mapBox = await page.Locator(".gisora-map-wrapper").BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(mapBox);
        Assert.True(mapBox.Height > 600, $"Mapa nao ocupou a tela: {mapBox.Height}px.");

        var properties = page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Imoveis", Exact = true });
        var facilities = page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Equipamentos", Exact = true });

        await Expect(properties).ToBeCheckedAsync();
        await Expect(facilities).ToBeCheckedAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(2);
        await Expect(page.Locator(".gisora-map .gisora-map-pin")).ToHaveCountAsync(2);

        await properties.UncheckAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(0);

        await page.Locator(".cadastro-bar__item[aria-label='Imoveis']").ClickAsync();
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/imoveis$"));
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Imoveis");

        await Expect(properties).ToBeDisabledAsync();
        await Expect(properties).ToBeCheckedAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(2);

        var lockedRow = page.Locator(".gisora-layer-control__item--locked");
        await Expect(lockedRow).ToContainTextAsync("Imoveis");
        await Expect(lockedRow).ToHaveAttributeAsync("title", "Camada bloqueada enquanto a feicao esta em cadastro ou edicao.");

        await page.Locator(".cadastro-bar__item[aria-label='Quadras']").ClickAsync();
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));

        await Expect(properties).ToBeEnabledAsync();
        await Expect(properties).Not.ToBeCheckedAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(0);

        await properties.CheckAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task RedirectsInvalidModuleToQuadras()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/desconhecido");

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Quadras");
    }

    private async Task SeedAsync(IPage page)
    {
        await page.GotoAsync(fixture.BaseUrl);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");
    }
}