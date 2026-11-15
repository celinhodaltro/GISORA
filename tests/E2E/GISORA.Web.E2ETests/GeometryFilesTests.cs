using System.Text;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class GeometryFilesTests(WebAppFixture fixture)
{
    [Fact]
    public async Task ExportsPropertyGeoJsonAndReimportsIt()
    {
        var page = await fixture.NewPageAsync();
        await SeedPropertyWithGeometryAsync(page);

        var content = await DownloadAsync(page, "Exportar GeoJSON");

        Assert.Contains("\"type\":\"FeatureCollection\"", content, StringComparison.Ordinal);
        Assert.Contains("\"inscricao\":\"001.002.003\"", content, StringComparison.Ordinal);
        Assert.Contains("\"targetCrs\":\"EPSG:4326\"", content, StringComparison.Ordinal);
        Assert.Contains("\"exportedAt\"", content, StringComparison.Ordinal);

        await page.GotoAsync($"{fixture.BaseUrl}/data");
        await ImportAsync(page, "imovel.geojson", content);

        await Expect(page.GetByText("1 imovel(is) atualizado(s)")).ToBeVisibleAsync();
        await Expect(page.Locator(".rz-data-grid")).ToContainTextAsync("imovel.geojson");
    }

    [Fact]
    public async Task ExportsMapLayerAndReimportsIt()
    {
        var page = await fixture.NewPageAsync();
        await SeedPropertyWithGeometryAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/map");
        await Expect(page.Locator("path.leaflet-interactive")).ToHaveCountAsync(1);
        await page.GetByLabel("Camada para exportar").SelectOptionAsync("properties");

        var content = await DownloadAsync(page, "Exportar GeoJSON");

        Assert.Contains("\"type\":\"FeatureCollection\"", content, StringComparison.Ordinal);
        Assert.Contains("\"inscricao\":\"001.002.003\"", content, StringComparison.Ordinal);

        await page.GotoAsync($"{fixture.BaseUrl}/data");
        await ImportAsync(page, "camada.imoveis.geojson", content);

        await Expect(page.GetByText("1 imovel(is) atualizado(s)")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RejectsInvalidGeoJsonFileWithReason()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/data");

        await ImportAsync(page, "invalido.geojson", "isto nao e geojson");

        await Expect(page.Locator(".workspace__error")).ToContainTextAsync("GeoJSON invalido");
        await Expect(page.GetByText("imovel(is) atualizado(s)")).ToHaveCountAsync(0);
    }

    private static async Task<string> DownloadAsync(IPage page, string buttonName)
    {
        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = buttonName }).ClickAsync());

        var path = await download.PathAsync();
        ArgumentNullException.ThrowIfNull(path);

        return await File.ReadAllTextAsync(path);
    }

    private static async Task ImportAsync(IPage page, string fileName, string content)
    {
        await page.GetByLabel("Arquivo GeoJSON").SetInputFilesAsync(new FilePayload
        {
            Name = fileName,
            MimeType = "application/geo+json",
            Buffer = Encoding.UTF8.GetBytes(content),
        });

        await Expect(page.GetByText($"Arquivo: {fileName}")).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Importar GeoJSON" }).ClickAsync();
    }

    private async Task SeedPropertyWithGeometryAsync(IPage page)
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

        await page.GotoAsync($"{fixture.BaseUrl}/properties/new");
        await page.GetByPlaceholder("Setor").FillAsync("001");
        await page.GetByPlaceholder("Quadra").FillAsync("002");
        await page.GetByPlaceholder("Lote").FillAsync("003");
        await page.GetByPlaceholder("Numero").FillAsync("10");
        await page.GetByLabel("Area do lote (m2)").FillAsync("500");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar imovel" }).ClickAsync();
        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Geometria" }).First.ClickAsync();
        await Expect(page.Locator(".geometry-page__registration")).ToHaveCountAsync(1);
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();

        await MapDrawing.StartAsync(page, "Poligono");
        await ClickMapAsync(page, 0.30, 0.30);
        await ClickMapAsync(page, 0.65, 0.30);
        await ClickMapAsync(page, 0.65, 0.65);
        await ClickMapAsync(page, 0.30, 0.65);
        await MapDrawing.FinishAsync(page);
        await Expect(page.GetByText("Area calculada")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar geometria" }).ClickAsync();
        await Expect(page.GetByText("Geometria atualizada.")).ToBeVisibleAsync();
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