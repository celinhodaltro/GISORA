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

        await page.ReloadAsync();

        await Expect(page.GetByText("Sede", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
        await Expect(page.GetByText("Centro", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
        await Expect(page.GetByText("001", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MunicipalityRequiresSelectionFromCatalog()
    {
        var page = await fixture.NewPageAsync();
        await IbgeCacheSeed.SeedAsync(page, fixture.BaseUrl);
        await page.GotoAsync($"{fixture.BaseUrl}/territory");

        await page.GetByPlaceholder("Digite o nome do municipio").FillAsync("Municipio Inexistente");

        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Adicionar municipio" })).ToBeDisabledAsync();
        await Expect(page.GetByText("Municipio Inexistente", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AutocompleteFiltersByPartialNameIgnoringCaseAndAccents()
    {
        var page = await fixture.NewPageAsync();
        await IbgeCacheSeed.SeedAsync(page, fixture.BaseUrl);
        await page.GotoAsync($"{fixture.BaseUrl}/territory");

        await page.GetByPlaceholder("Digite o nome do municipio").FillAsync("pass");

        await Expect(page.Locator(".territory-suggestions button", new PageLocatorOptions { HasTextString = "Passira" })).ToBeVisibleAsync();
        await Expect(page.Locator(".territory-suggestions button", new PageLocatorOptions { HasTextString = "Afogados" })).ToHaveCountAsync(0);
    }
}