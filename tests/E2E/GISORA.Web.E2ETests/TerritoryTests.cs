using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class TerritoryTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesFullTerritoryHierarchyAndKeepsItAfterReload()
    {
        var page = await fixture.NewPageAsync();
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

        await page.GetByPlaceholder("Codigo do setor").FillAsync("001");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar setor" }).ClickAsync();
        await Expect(page.GetByText("001", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();

        await page.ReloadAsync();

        await Expect(page.GetByText("Sede", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
        await Expect(page.GetByText("Centro", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
        await Expect(page.GetByText("001", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RejectsInvalidIbgeCode()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/territory");

        await page.GetByPlaceholder("Nome do municipio").FillAsync("Invalido");
        await page.GetByPlaceholder("Codigo IBGE").FillAsync("123");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar municipio" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Codigo IBGE deve conter exatamente 7 digitos.");
    }
}