using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class GeometryDrawingTests(WebAppFixture fixture)
{
    [Fact]
    public async Task DrawsGeometryValidatesAreaAndShowsPropertyOnMap()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Geometria" }).First.ClickAsync();
        await Expect(page.Locator(".geometry-page__registration")).ToHaveCountAsync(1);
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar" }).ClickAsync();
        await ClickMapAsync(page, 0.30, 0.30);
        await ClickMapAsync(page, 0.65, 0.30);
        await ClickMapAsync(page, 0.65, 0.65);
        await ClickMapAsync(page, 0.30, 0.65);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir desenho" }).ClickAsync();

        await Expect(page.GetByText("Area calculada")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" }).ClickAsync();
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Editar geometria" })).ToBeEnabledAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Editar geometria" }).ClickAsync();
        await Expect(page.Locator(".gisora-map-vertex")).ToHaveCountAsync(4);

        var handle = page.Locator(".gisora-map-vertex").First;
        await handle.HoverAsync();
        var handleBox = await handle.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(handleBox);
        var startX = handleBox.X + (handleBox.Width / 2);
        var startY = handleBox.Y + (handleBox.Height / 2);
        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(startX + 6, startY + 6, new MouseMoveOptions { Steps = 5 });
        await Task.Delay(120);
        await page.Mouse.MoveAsync(startX + 60, startY + 45, new MouseMoveOptions { Steps = 20 });
        await Task.Delay(120);
        await page.Mouse.UpAsync();
        await Expect(page.GetByText("Area calculada")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" }).ClickAsync();
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/map");
        await Expect(page.Locator("path.leaflet-interactive")).ToHaveCountAsync(1);
    }

    private static async Task ClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var box = await page.Locator(".gisora-map").BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
    }

    private async Task SeedAsync(IPage page)
    {
        await IbgeCacheSeed.SeedAsync(page, fixture.BaseUrl);
        await page.GotoAsync($"{fixture.BaseUrl}/territory");
        await IbgeCacheSeed.CreateMunicipalityAsync(page);

        await page.GetByPlaceholder("Nome do distrito").FillAsync("Sede");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar distrito" }).ClickAsync();
        await Expect(page.GetByText("Sede", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.GetByPlaceholder("Nome do bairro").FillAsync("Centro");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar bairro" }).ClickAsync();
        await Expect(page.GetByText("Centro", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/streets");
        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua da Matriz");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();
        await Expect(page.GetByText("Rua da Matriz", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.GotoAsync($"{fixture.BaseUrl}/properties/new");
        await page.GetByPlaceholder("Setor").FillAsync("001");
        await page.GetByPlaceholder("Quadra").FillAsync("002");
        await page.GetByPlaceholder("Lote").FillAsync("003");
        await page.GetByPlaceholder("Numero").FillAsync("10");
        await page.GetByLabel("Area do lote (m2)").FillAsync("500");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar imovel" }).ClickAsync();
        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }
}