using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class GeometryValidationTests(WebAppFixture fixture)
{
    [Fact]
    public async Task RejectsOverlappingGeometryBetweenProperties()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);
        await CreateSecondPropertyAsync(page);

        await DrawAndSaveAsync(page, 0);
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Geometria" }).Nth(1).ClickAsync();
        await Expect(page.Locator(".geometry-page__registration")).ToHaveCountAsync(1);

        await DrawAsync(page);

        var diagnostics = page.Locator(".geometry-page__diagnostics");
        await Expect(diagnostics).ToBeVisibleAsync();
        await Expect(diagnostics).ToContainTextAsync("sobrepoe");
        await Expect(diagnostics).ToContainTextAsync("001.002.003");
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" })).ToBeDisabledAsync();
        await Expect(page.GetByText("Geometria atualizada.")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AcceptsNonOverlappingGeometryBetweenProperties()
    {
        var page = await fixture.NewPageAsync();
        await SeedAsync(page);
        await CreateSecondPropertyAsync(page);

        await DrawAndSaveAsync(page, 0);
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Geometria" }).Nth(1).ClickAsync();
        await Expect(page.Locator(".geometry-page__registration")).ToHaveCountAsync(1);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar poligono" }).ClickAsync();
        await ClickMapAsync(page, 0.60, 0.60);
        await ClickMapAsync(page, 0.90, 0.60);
        await ClickMapAsync(page, 0.90, 0.90);
        await ClickMapAsync(page, 0.60, 0.90);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir desenho" }).ClickAsync();

        await Expect(page.GetByText("Area calculada")).ToBeVisibleAsync();
        await Expect(page.Locator(".geometry-page__diagnostics")).ToHaveCountAsync(0);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" }).ClickAsync();
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();
    }

    private async Task DrawAndSaveAsync(IPage page, int propertyIndex)
    {
        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Geometria" }).Nth(propertyIndex).ClickAsync();
        await Expect(page.Locator(".geometry-page__registration")).ToHaveCountAsync(1);

        await DrawPolygonAsync(page, 0.10, 0.10, 0.40, 0.40);

        await Expect(page.GetByText("Area calculada")).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" }).ClickAsync();
    }

    private static async Task DrawAsync(IPage page) => await DrawPolygonAsync(page, 0.30, 0.30, 0.65, 0.65);

    private static async Task DrawPolygonAsync(IPage page, double left, double top, double right, double bottom)
    {
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar poligono" }).ClickAsync();
        await ClickMapAsync(page, left, top);
        await ClickMapAsync(page, right, top);
        await ClickMapAsync(page, right, bottom);
        await ClickMapAsync(page, left, bottom);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir desenho" }).ClickAsync();
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
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar distrito" }).ClickAsync();
        await Expect(page.GetByText("Sede", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.GetByPlaceholder("Nome do bairro").FillAsync("Centro");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar bairro" }).ClickAsync();
        await Expect(page.GetByText("Centro", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/streets");
        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua da Matriz");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();
        await Expect(page.GetByText("Rua da Matriz", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await CreatePropertyAsync(page, "003");
        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    private async Task CreateSecondPropertyAsync(IPage page)
    {
        await CreatePropertyAsync(page, "004");
        await Expect(page.GetByText("001.002.004", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    private async Task CreatePropertyAsync(IPage page, string lot)
    {
        await page.GotoAsync($"{fixture.BaseUrl}/properties/new");
        await page.GetByPlaceholder("Setor").FillAsync("001");
        await page.GetByPlaceholder("Quadra").FillAsync("002");
        await page.GetByPlaceholder("Lote").FillAsync(lot);
        await page.GetByPlaceholder("Numero").FillAsync("10");
        await page.GetByLabel("Area do lote (m2)").FillAsync("500");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar imovel" }).ClickAsync();
    }
}