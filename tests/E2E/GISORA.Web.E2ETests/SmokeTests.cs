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
        await Expect(page.Locator(".nav-menu__brand")).ToHaveTextAsync("GISORA");
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Imoveis" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Visao geral" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NavigatesToPropertiesAndShowsEmptyState()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Imoveis" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties$"));
        await Expect(page.GetByText("Nenhum imovel cadastrado neste dispositivo.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SchemaVersionIsPersistedOnFirstRun()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".nav-menu__brand")).ToBeVisibleAsync();

        var schema = await page.EvaluateAsync<string?>("() => localStorage.getItem('gisora:meta:schema')");

        Assert.Equal("1", schema);
    }

    [Fact]
    public async Task ReloadDoesNotShowErrorUi()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await Expect(page.Locator(".nav-menu__brand")).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Expect(page.Locator(".nav-menu__brand")).ToBeVisibleAsync();

        await Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }
}