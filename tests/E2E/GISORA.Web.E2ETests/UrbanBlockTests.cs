using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class UrbanBlockTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesBlockWithGeometryAndFaceThenSelectsItInPropertyForm()
    {
        var page = await fixture.NewPageAsync();
        await SeedTerritoryAsync(page);

        await page.GotoAsync($"{fixture.BaseUrl}/urban-blocks");
        await Expect(page.GetByLabel("Setor fiscal da quadra")).ToBeVisibleAsync();

        await page.GetByPlaceholder("Codigo da quadra").FillAsync("Q-10");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar contorno da quadra" }).ClickAsync();
        await ClickMapAsync(page, 0.30, 0.30);
        await ClickMapAsync(page, 0.65, 0.30);
        await ClickMapAsync(page, 0.65, 0.65);
        await ClickMapAsync(page, 0.30, 0.65);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir contorno da quadra" }).ClickAsync();

        await Expect(page.GetByText("Contorno desenhado com")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar quadra" }).ClickAsync();

        await Expect(page.GetByText("Quadra cadastrada.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Q-10", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
        await Expect(page.Locator(".gisora-map path.leaflet-interactive").First).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Abrir quadra Q-10" }).ClickAsync();
        await Expect(page.GetByText("Quadra Q-10").First).ToBeVisibleAsync();

        await page.GetByLabel("Logradouro da face").SelectOptionAsync(new SelectOptionValue { Label = "Rua da Matriz" });
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Desenhar linha da face" }).ClickAsync();
        await ClickMapAsync(page, 0.35, 0.55);
        await ClickMapAsync(page, 0.60, 0.55);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concluir linha da face" }).ClickAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar face" }).ClickAsync();

        await Expect(page.GetByText("Face adicionada.")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "Rua da Matriz" })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Excluir quadra" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Confirmar exclusao" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Quadra possui faces vinculadas e nao pode ser excluida.");

        await page.GotoAsync($"{fixture.BaseUrl}/map");
        await Expect(page.Locator("path.leaflet-interactive")).ToHaveCountAsync(2);

        await page.GotoAsync($"{fixture.BaseUrl}/properties/new");
        await page.GetByLabel("Quadra").SelectOptionAsync(new SelectOptionValue { Label = "001 - Q-10" });
        await Expect(page.GetByPlaceholder("Setor")).ToHaveValueAsync("001");
        await page.GetByPlaceholder("Lote").FillAsync("003");
        await page.GetByPlaceholder("Numero").FillAsync("10");
        await page.GetByLabel("Area do lote (m2)").FillAsync("500");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar imovel" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties$"));
        await Expect(page.GetByText("001.Q-10.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.ReloadAsync();
        await Expect(page.GetByText("001.Q-10.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    private static async Task ClickMapAsync(IPage page, double relativeX, double relativeY)
    {
        var map = page.Locator(".gisora-map");
        await map.ScrollIntoViewIfNeededAsync();

        var box = await map.BoundingBoxAsync();
        ArgumentNullException.ThrowIfNull(box);

        await page.Mouse.ClickAsync(
            box.X + (box.Width * (float)relativeX),
            box.Y + (box.Height * (float)relativeY));
    }

    private async Task SeedTerritoryAsync(IPage page)
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

        await page.GetByPlaceholder("Codigo do setor").FillAsync("001");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar setor" }).ClickAsync();
        await Expect(page.GetByText("001", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.GotoAsync($"{fixture.BaseUrl}/streets");
        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua da Matriz");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();
        await Expect(page.GetByText("Rua da Matriz", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }
}