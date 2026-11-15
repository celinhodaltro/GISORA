using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class SmokeTests(WebAppFixture fixture)
{
    [Fact]
    public async Task HomeLoadsWithBrandAndNavigation()
    {
        var page = await fixture.NewPageAsync();

        await page.GotoAsync(fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await Expect(page).ToHaveTitleAsync("GISORA");
        await Expect(page.Locator(".app-brand")).ToContainTextAsync("GISORA");
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cadastro no mapa" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Painel territorial" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NavigatesToUnifiedCadastroAndShowsMap()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cadastro no mapa" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/cadastro/quadras$"));
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();
        await Expect(page.Locator(".cadastro-bar__item")).ToHaveCountAsync(4);
        await Expect(page.GetByText("Cadastro territorial", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(page.Locator(".app-nav").GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Imoveis", Exact = true })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task SchemaVersionIsPersistedOnFirstRun()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".app-brand")).ToBeVisibleAsync();

        var schema = await page.EvaluateAsync<string?>("() => localStorage.getItem('gisora:meta:schema')");

        Assert.Equal("1", schema);
    }

    [Fact]
    public async Task ReloadDoesNotShowErrorUi()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await Expect(page.Locator(".app-brand")).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Expect(page.Locator(".app-brand")).ToBeVisibleAsync();

        await Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }
}