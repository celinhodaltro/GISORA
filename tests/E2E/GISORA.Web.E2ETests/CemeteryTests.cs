using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class CemeteryTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesCemeteryGraveBurialAndExhumation()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/cemetery");

        await page.GetByPlaceholder("Nome do cemiterio").FillAsync("Cemiterio Municipal");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar limite" }).ClickAsync();
        await ClickMapAsync(page, 0.35, 0.35);
        await ClickMapAsync(page, 0.65, 0.35);
        await ClickMapAsync(page, 0.65, 0.65);
        await ClickMapAsync(page, 0.35, 0.65);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir desenho" }).ClickAsync();

        await Expect(page.GetByText("Area:")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar cemiterio" }).ClickAsync();
        await Expect(page.GetByText("Cemiterio cadastrado.")).ToBeVisibleAsync();
        await Expect(page.Locator("select[aria-label='Cemiterio']")).ToContainTextAsync("Cemiterio Municipal");

        await page.GetByPlaceholder("Codigo do setor").FillAsync("A");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar setor" }).ClickAsync();
        await Expect(page.GetByText("Setor cadastrado.")).ToBeVisibleAsync();

        await page.GetByPlaceholder("Codigo do jazigo").FillAsync("J-01");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Marcar jazigo" }).ClickAsync();

        var box = await page.Locator(".gisora-map").BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);
        await page.Mouse.ClickAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));

        await Expect(page.Locator(".cemetery-coordinate")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar jazigo" }).ClickAsync();
        await Expect(page.GetByText("Jazigo cadastrado.")).ToBeVisibleAsync();

        await page.GetByPlaceholder("Nome da pessoa").FillAsync("Maria");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sepultar" }).ClickAsync();
        await Expect(page.GetByText("Sepultamento registrado.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Ocupado", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(page.GetByText("Maria", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sepultar" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Jazigo ja possui sepultamento ativo.");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Exumar" }).ClickAsync();
        await Expect(page.GetByText("Livre", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    private static async Task ClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var box = await page.Locator(".gisora-map").BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
    }
}