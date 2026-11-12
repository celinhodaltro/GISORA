using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class VisualIdentityTests(WebAppFixture fixture)
{
    [Fact]
    public async Task ShellShowsInstitutionalHeaderAndDomainNavigation()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".app-brand")).ToContainTextAsync("GISORA");
        await Expect(page.Locator(".app-brand__mark img")).ToBeVisibleAsync();
        await Expect(page.Locator(".app-header__env")).ToContainTextAsync("Base local");
        await Expect(page.Locator(".app-header__municipality")).ToHaveTextAsync("Nao definido");
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Territorio" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Mapa" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MenuCollapsesAndExpands()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".app-shell")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("app-shell--collapsed"));

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Alternar menu" }).ClickAsync();
        await Expect(page.Locator(".app-shell")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("app-shell--collapsed"));

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Alternar menu" }).ClickAsync();
        await Expect(page.Locator(".app-shell")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("app-shell--collapsed"));
    }

    [Fact]
    public async Task MobileLayoutHasNoHorizontalOverflowAndOpensDrawer()
    {
        var page = await fixture.NewPageAsync();
        await page.SetViewportSizeAsync(375, 700);
        await page.GotoAsync(fixture.BaseUrl);

        var overflow = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

        Assert.False(overflow, "A pagina apresentou rolagem horizontal em 375px.");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Alternar menu" }).ClickAsync();
        await Expect(page.Locator(".app-nav")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("app-nav--open"));

        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Mapa" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/map$"));
    }

    [Fact]
    public async Task HomeShowsMapLegendAfterLoadingDemoData()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await Expect(page.Locator(".gisora-map-legend")).ToContainTextAsync("Imoveis");
        await Expect(page.Locator(".gisora-map-legend")).ToContainTextAsync("Equipamentos");
        await Expect(page.Locator(".leaflet-container")).ToBeVisibleAsync();
    }
}