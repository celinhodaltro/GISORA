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
    public async Task CreatesPolygonDraftFromFloatingAction()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/imoveis");
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();

        var fab = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" });
        await Expect(fab).ToBeEnabledAsync();

        var viewport = page.ViewportSize;
        ArgumentNullException.ThrowIfNull(viewport);
        var fabBox = await fab.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(fabBox);
        Assert.True(fabBox.X + fabBox.Width > viewport.Width - 80, "O + nao esta no canto inferior direito.");
        Assert.True(fabBox.Y + fabBox.Height > viewport.Height - 100, "O + nao esta no canto inferior direito.");

        await fab.ClickAsync();
        await Expect(fab).ToBeDisabledAsync();
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = MapDrawing.Finish })).ToBeVisibleAsync();

        await ClickMapAsync(page, 0.30, 0.30);
        await ClickMapAsync(page, 0.65, 0.30);
        await ClickMapAsync(page, 0.65, 0.65);
        await ClickMapAsync(page, 0.30, 0.65);
        await MapDrawing.FinishAsync(page);

        await Expect(page.Locator(".cadastro-draft")).ToContainTextAsync("Rascunho de Imoveis");
        await Expect(page.Locator(".cadastro-draft")).ToContainTextAsync("4 vertice");
        await Expect(fab).ToBeEnabledAsync();
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = MapDrawing.Finish })).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(1);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Descartar rascunho" }).ClickAsync();

        await Expect(page.Locator(".cadastro-draft")).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task CreatesFacilityDraftFromTypeIcons()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/equipamentos");
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();

        var fab = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" });
        await fab.ClickAsync();

        await Expect(page.Locator(".cadastro-types")).ToBeVisibleAsync();
        await Expect(page.Locator(".cadastro-types__item")).ToHaveCountAsync(7);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Escola", Exact = true }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = MapDrawing.Cancel })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = MapDrawing.Finish })).ToHaveCountAsync(0);
        await Expect(fab).ToBeDisabledAsync();

        await ClickMapAsync(page, 0.5, 0.5);

        await Expect(page.Locator(".cadastro-draft")).ToContainTextAsync("Rascunho de equipamento: Escola");
        await Expect(page.Locator(".gisora-map .gisora-map-pin--school")).ToHaveCountAsync(1);
        await Expect(fab).ToBeEnabledAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Descartar rascunho" }).ClickAsync();

        await Expect(page.Locator(".cadastro-draft")).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-map .gisora-map-pin--school")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task RedirectsInvalidModuleToQuadras()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/desconhecido");

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Quadras");
    }

    private static async Task ClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var map = page.Locator(".gisora-map");
        var box = await map.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
    }

    private async Task SeedAsync(IPage page)
    {
        await page.GotoAsync(fixture.BaseUrl);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");
    }
}