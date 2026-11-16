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
    public async Task CreatesPropertyFromDraftAndEditsGeometry()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/imoveis");
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/imoveis$"));
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Imoveis");

        for (var index = 0; index < 3; index++)
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Afastar" }).ClickAsync();
        }

        await DrawPolygonDraftAsync(page, 0.05, 0.62, 0.22, 0.80);

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/imoveis$"));
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("Rascunho de Imoveis");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();

        var modal = page.Locator(".cadastro-modal");
        await Expect(modal).ToBeVisibleAsync();

        await modal.Locator(".cadastro-modal__button--primary").ClickAsync();
        await Expect(modal.Locator(".cadastro-modal__error")).ToContainTextAsync("Selecione logradouro e bairro.");
        await Expect(modal.Locator(".cadastro-modal__error")).Not.ToContainTextAsync("_modalError");

        await modal.GetByLabel("Setor", new LocatorGetByLabelOptions { Exact = true }).FillAsync("001");
        await modal.GetByLabel("Quadra", new LocatorGetByLabelOptions { Exact = true }).FillAsync("010");
        await modal.GetByLabel("Lote", new LocatorGetByLabelOptions { Exact = true }).FillAsync("001");
        await modal.GetByLabel("Logradouro").SelectOptionAsync(new SelectOptionValue { Label = "Rua da Matriz" });
        await modal.GetByLabel("Numero").FillAsync("10");
        await modal.GetByLabel("Bairro").SelectOptionAsync(new SelectOptionValue { Label = "Centro" });

        await modal.Locator(".cadastro-modal__button--primary").ClickAsync();

        await Expect(modal).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("001.010.001");
        await Expect(page.Locator(".gisora-map path.leaflet-interactive")).ToHaveCountAsync(3);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Editar geometria" }).ClickAsync();
        await Expect(page.Locator(".cadastro-edit")).ToContainTextAsync("Editando geometria");
        await Expect(page.Locator(".cadastro-draft")).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-map-vertex")).ToHaveCountAsync(4);

        var saveButton = page.Locator(".cadastro-edit__save");
        var cancelButton = page.Locator(".cadastro-edit__cancel");
        var saveBox = await saveButton.BoundingBoxAsync();
        var cancelBox = await cancelButton.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(saveBox);
        ArgumentNullException.ThrowIfNull(cancelBox);
        Assert.True(saveBox.Y < cancelBox.Y, "Salvar deve ficar acima de Cancelar.");

        var handle = page.Locator(".gisora-map-vertex").First;
        var handleBox = await handle.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(handleBox);
        var startX = handleBox.X + (handleBox.Width / 2);
        var startY = handleBox.Y + (handleBox.Height / 2);
        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(startX + 20, startY + 15, new MouseMoveOptions { Steps = 8 });
        await page.Mouse.UpAsync();

        await saveButton.ClickAsync();

        await Expect(page.Locator(".cadastro-edit")).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("001.010.001");
    }

    [Fact]
    public async Task DrawsPropertyOverExistingBlockWithoutOpeningBlockPopup()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/quadras");
        await DrawPolygonDraftAsync(page);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();

        var blockModal = page.Locator(".cadastro-modal");
        await blockModal.GetByLabel("Codigo").FillAsync("Q-90");
        await blockModal.Locator(".cadastro-modal__button--primary").ClickAsync();
        await Expect(blockModal).ToHaveCountAsync(0);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/imoveis");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" }).ClickAsync();
        await ClickMapAsync(page, 0.30, 0.30);
        await ClickMapAsync(page, 0.65, 0.30);
        await ClickMapAsync(page, 0.65, 0.65);
        await DoubleClickMapAsync(page, 0.30, 0.65);

        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("Rascunho de Imoveis");
        await Expect(page.Locator(".gisora-popup")).Not.ToContainTextAsync("Q-90");
    }

    [Fact]
    public async Task SelectsPropertyAboveBlockAndShowsSelectionAffordances()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/quadras");

        var propertyPath = page.Locator(".gisora-map path[stroke='#1f7a5c']").First;
        var propertyBox = await propertyPath.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(propertyBox);

        var centerX = propertyBox.X + (propertyBox.Width / 2);
        var centerY = propertyBox.Y + (propertyBox.Height / 2);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" }).ClickAsync();
        await page.Mouse.ClickAsync(centerX - 25, centerY - 25);
        await page.Mouse.ClickAsync(centerX + 25, centerY - 25);
        await page.Mouse.ClickAsync(centerX + 25, centerY + 25);
        await page.Mouse.ClickAsync(centerX - 25, centerY + 25);
        await MapDrawing.FinishAsync(page);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();
        var blockModal = page.Locator(".cadastro-modal");
        await blockModal.GetByLabel("Codigo").FillAsync("Q-91");
        await blockModal.Locator(".cadastro-modal__button--primary").ClickAsync();
        await Expect(blockModal).ToHaveCountAsync(0);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/imoveis");

        var property = page.Locator(".gisora-map path[stroke='#1f7a5c']").First;
        var box = await property.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));

        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("Inscricao");
        await Expect(page.Locator(".gisora-popup")).Not.ToContainTextAsync("Q-91");
        await Expect(page.Locator(".gisora-map path[stroke='#f59e0b']")).ToHaveCountAsync(1);
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Editar feicao" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatesPropertyWithMultiplePolygons()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/imoveis");

        for (var index = 0; index < 3; index++)
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Afastar" }).ClickAsync();
        }

        await DrawPolygonDraftAsync(page, 0.05, 0.62, 0.20, 0.75);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar parte" }).ClickAsync();
        await ClickMapAsync(page, 0.30, 0.62);
        await ClickMapAsync(page, 0.45, 0.62);
        await ClickMapAsync(page, 0.45, 0.75);
        await ClickMapAsync(page, 0.30, 0.75);
        await MapDrawing.FinishAsync(page);

        await Expect(page.Locator(".cadastro-draft")).ToContainTextAsync("2 partes");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();

        var modal = page.Locator(".cadastro-modal");
        await Expect(modal).ToContainTextAsync("multi-poligono com 2 parte(s)");

        await modal.GetByLabel("Setor", new LocatorGetByLabelOptions { Exact = true }).FillAsync("001");
        await modal.GetByLabel("Quadra", new LocatorGetByLabelOptions { Exact = true }).FillAsync("020");
        await modal.GetByLabel("Lote", new LocatorGetByLabelOptions { Exact = true }).FillAsync("001");
        await modal.GetByLabel("Logradouro").SelectOptionAsync(new SelectOptionValue { Label = "Rua da Matriz" });
        await modal.GetByLabel("Numero").FillAsync("10");
        await modal.GetByLabel("Bairro").SelectOptionAsync(new SelectOptionValue { Label = "Centro" });
        await modal.Locator(".cadastro-modal__button--primary").ClickAsync();

        await Expect(modal).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("001.020.001");
    }

    [Fact]
    public async Task CreatesFacilityFromDraftAndOpensModal()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/equipamentos");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Escola", Exact = true }).ClickAsync();
        await ClickMapAsync(page, 0.55, 0.45);

        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("TipoEscola");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();

        var modal = page.Locator(".cadastro-modal");
        await Expect(modal).ToBeVisibleAsync();

        await modal.GetByLabel("Nome").FillAsync("Escola Nova");
        await modal.GetByLabel("Bairro").SelectOptionAsync(new SelectOptionValue { Label = "Centro" });
        await modal.Locator(".cadastro-modal__button--primary").ClickAsync();

        await Expect(modal).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("Escola Nova");
        await Expect(page.Locator(".gisora-map .gisora-map-pin--school")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task CreatesBlockFromDraftWithSectorAndCode()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/quadras");
        await DrawPolygonDraftAsync(page);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir cadastro" }).ClickAsync();

        var modal = page.Locator(".cadastro-modal");
        await Expect(modal).ToBeVisibleAsync();

        await modal.GetByLabel("Codigo").FillAsync("Q-77");
        await modal.Locator(".cadastro-modal__button--primary").ClickAsync();

        await Expect(modal).ToHaveCountAsync(0);
        await Expect(page.Locator(".gisora-popup")).ToContainTextAsync("Q-77");
    }

    private static async Task DrawPolygonDraftAsync(
        IPage page,
        double left = 0.30,
        double top = 0.30,
        double right = 0.65,
        double bottom = 0.65)
    {
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Novo cadastro" }).ClickAsync();
        await ClickMapAsync(page, left, top);
        await ClickMapAsync(page, right, top);
        await ClickMapAsync(page, right, bottom);
        await ClickMapAsync(page, left, bottom);
        await MapDrawing.FinishAsync(page);
    }

    [Fact]
    public async Task RedirectsInvalidModuleToQuadras()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/desconhecido");

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));
        await Expect(page.Locator(".cadastro-bar__item[aria-current='page']")).ToHaveAttributeAsync("aria-label", "Quadras");
    }

    private static async Task DoubleClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var map = page.Locator(".gisora-map");
        var box = await map.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.DblClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
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