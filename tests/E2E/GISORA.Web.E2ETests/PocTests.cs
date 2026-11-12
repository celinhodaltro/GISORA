using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class PocTests(WebAppFixture fixture)
{
    [Fact]
    public async Task LoadsDemoDataAndShowsItAcrossTheApplication()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".poc-banner")).ToContainTextAsync("POC local");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await Expect(CardValue(page, "Imoveis")).ToHaveTextAsync("3");
        await Expect(CardValue(page, "Logradouros")).ToHaveTextAsync("2");
        await Expect(CardValue(page, "Equipamentos")).ToHaveTextAsync("2");

        await page.GotoAsync($"{fixture.BaseUrl}/properties");
        await Expect(page.GetByText("001.002.003", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(page.GetByText("001.002.004", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(page.GetByText("001.003.010", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.GotoAsync($"{fixture.BaseUrl}/map");
        await Expect(page.Locator("path.leaflet-interactive")).ToHaveCountAsync(2);

        await page.GotoAsync($"{fixture.BaseUrl}/facilities");
        await Expect(page.Locator(".leaflet-marker-icon")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task SecondSeedAttemptReportsAlreadyLoaded()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        var seedButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" });
        await seedButton.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await seedButton.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("ja estava carregada");
    }

    private static ILocator CardValue(IPage page, string label)
        => page.Locator(".gisora-stat-card", new PageLocatorOptions { HasTextString = label })
            .Locator(".gisora-stat-card__value");
}