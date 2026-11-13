using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class RuralTests(WebAppFixture fixture)
{
    private const string ImportedGeoJson =
        "{\"type\":\"Polygon\",\"coordinates\":[[[-35.262,-7.962],[-35.242,-7.962],[-35.242,-7.942],[-35.262,-7.942],[-35.262,-7.962]]]}";

    [Fact]
    public async Task CreatesRuralPropertyFromMapBoundary()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/rural");

        await page.GetByPlaceholder("Nome da propriedade").FillAsync("Fazenda Boa Vista");
        await page.GetByPlaceholder("Codigo CAR").FillAsync("PE-2610509-ABCD.EFGH.IJKL.MNOP.QRST");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar limite" }).ClickAsync();

        await ClickMapAsync(page, 0.3, 0.3);
        await ClickMapAsync(page, 0.7, 0.3);
        await ClickMapAsync(page, 0.7, 0.7);
        await ClickMapAsync(page, 0.3, 0.7);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir desenho" }).ClickAsync();

        await Expect(page.Locator(".rural-area")).ToContainTextAsync("ha");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar propriedade" }).ClickAsync();

        await Expect(page.GetByText("Propriedade rural cadastrada.")).ToBeVisibleAsync();
        await Expect(page.Locator(".rz-data-grid")).ToContainTextAsync("Fazenda Boa Vista");
        await Expect(page.GetByText("PE-2610509-ABCD.EFGH.IJKL.MNOP.QRST")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task ImportsRuralPropertyFromGeoJson()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/rural");

        await page.GetByPlaceholder("Nome importado").FillAsync("Fazenda Importada");
        await page.GetByPlaceholder("GeoJSON").FillAsync(ImportedGeoJson);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Importar GeoJSON" }).ClickAsync();

        await Expect(page.GetByText("Propriedade rural importada.")).ToBeVisibleAsync();
        await Expect(page.Locator(".rz-data-grid")).ToContainTextAsync("Fazenda Importada");
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