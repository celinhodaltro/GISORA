using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

internal static class IbgeCacheSeed
{
    public static async Task SeedAsync(IPage page, string baseUrl)
    {
        await page.GotoAsync(baseUrl);

        await page.EvaluateAsync("""
            (data) => {
              localStorage.setItem('gisora:ibge:municipalities-PE', JSON.stringify({
                cachedAtUtc: data.cachedAtUtc,
                items: data.items
              }));
            }
            """, new
        {
            cachedAtUtc = DateTimeOffset.UtcNow.ToString("o"),
            items = new[]
            {
                new { code = 2610509, name = "Passira", stateAbbreviation = "PE" },
                new { code = 2600104, name = "Afogados da Ingazeira", stateAbbreviation = "PE" },
            },
        });
    }

    public static async Task CreateMunicipalityAsync(IPage page, string name = "Passira")
    {
        await page.GetByPlaceholder("Digite o nome do municipio").FillAsync(name);
        await page.Locator(".territory-suggestions button", new PageLocatorOptions { HasTextString = name }).First.ClickAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar municipio" }).ClickAsync();
        await Expect(page.GetByText(name, new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
    }
}