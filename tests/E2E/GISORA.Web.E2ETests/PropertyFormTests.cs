using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class PropertyFormTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesPropertyThroughFormAndKeepsItAfterReload()
    {
        var page = await fixture.NewPageAsync();
        await SeedTerritoryAsync(page);

        await CreatePropertyAsync(page, "001", "002", "003", "10", "500");

        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.ReloadAsync();

        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task RejectsDuplicatedRegistrationAndInvalidArea()
    {
        var page = await fixture.NewPageAsync();
        await SeedTerritoryAsync(page);

        await CreatePropertyAsync(page, "001", "002", "003", "10", "500");

        await OpenFormAsync(page, "001", "002", "003", "10", "500");
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Ja existe imovel com esta inscricao.");

        await OpenFormAsync(page, "001", "004", "005", "20", "0");
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Area do lote deve ser maior que zero.");
    }

    private async Task CreatePropertyAsync(
        IPage page,
        string sector,
        string block,
        string lot,
        string number,
        string area)
    {
        await OpenFormAsync(page, sector, block, lot, number, area);
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties$"));
    }

    private async Task OpenFormAsync(
        IPage page,
        string sector,
        string block,
        string lot,
        string number,
        string area)
    {
        await page.GotoAsync($"{fixture.BaseUrl}/properties/new");

        await page.GetByPlaceholder("Setor").FillAsync(sector);
        await page.GetByPlaceholder("Quadra").FillAsync(block);
        await page.GetByPlaceholder("Lote").FillAsync(lot);
        await page.GetByPlaceholder("Numero").FillAsync(number);
        await page.GetByLabel("Area do lote (m2)").FillAsync(area);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Salvar imovel" }).ClickAsync();
    }

    private async Task SeedTerritoryAsync(IPage page)
    {
        await page.GotoAsync($"{fixture.BaseUrl}/territory");
        await page.GetByPlaceholder("Nome do municipio").FillAsync("Passira");
        await page.GetByPlaceholder("Codigo IBGE").FillAsync("2611903");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar municipio" }).ClickAsync();
        await Expect(page.GetByText("Passira", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

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
    }
}