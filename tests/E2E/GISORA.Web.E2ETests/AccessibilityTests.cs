using System.Text.RegularExpressions;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class AccessibilityTests(WebAppFixture fixture)
{
    [Fact]
    public async Task SkipLinkMovesFocusToMainContent()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.Locator(".app-skip-link")).ToBeAttachedAsync();
        await page.WaitForTimeoutAsync(400);

        var firstFocusable = await page.EvaluateAsync<string>("""
            () => {
                const elements = [...document.querySelectorAll('a[href],button,input:not([type=hidden]),select,textarea')]
                    .filter(element => {
                        const style = window.getComputedStyle(element);
                        const rect = element.getBoundingClientRect();
                        return style.display !== 'none' && style.visibility !== 'hidden' && rect.width > 0 && rect.height > 0;
                    });

                return elements[0]?.className ?? '';
            }
            """);

        Assert.Contains("app-skip-link", firstFocusable, StringComparison.Ordinal);

        await page.Locator(".app-skip-link").FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("#main-content")).ToBeFocusedAsync();
    }

    [Fact]
    public async Task InteractiveElementsHaveAccessibleNames()
    {
        var page = await fixture.NewPageAsync();

        foreach (var path in new[] { string.Empty, "/data", "/territory", "/properties", "/cadastro/imoveis" })
        {
            await page.GotoAsync($"{fixture.BaseUrl}{path}");
            await page.WaitForTimeoutAsync(400);

            var missing = await page.EvaluateAsync<string[]>("""
                () => {
                    const selector = 'a,button,input:not([type=hidden]),select,textarea';
                    const missing = [];

                    for (const element of document.querySelectorAll(selector)) {
                        const style = window.getComputedStyle(element);
                        const rect = element.getBoundingClientRect();

                        if (style.display === 'none' || style.visibility === 'hidden' || rect.width === 0 || rect.height === 0) {
                            continue;
                        }

                        const aria = element.getAttribute('aria-label') || '';
                        const text = (element.innerText || element.value || '').trim();
                        const title = element.getAttribute('title') || '';
                        const alt = element.getAttribute('alt') || '';
                        const id = element.id;
                        let label = '';

                        if (id) {
                            const explicit = document.querySelector(`label[for="${id}"]`);
                            if (explicit) label = explicit.textContent || '';
                        }

                        if (!label) {
                            const wrapper = element.closest('label');
                            if (wrapper) label = wrapper.textContent || '';
                        }

                        if (!(aria || text || title || alt || label).trim()) {
                            missing.push(element.outerHTML.slice(0, 160));
                        }
                    }

                    return missing;
                }
                """);

            Assert.True(missing.Length == 0, $"Controles sem nome acessivel em {path}: {string.Join(" | ", missing)}");
        }
    }

    [Fact]
    public async Task MobileMenuOpensAndClosesAfterNavigation()
    {
        var page = await fixture.NewPageAsync();
        await page.SetViewportSizeAsync(390, 844);
        await page.GotoAsync(fixture.BaseUrl);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Alternar menu" }).ClickAsync();

        await Expect(page.Locator("#app-nav")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Imoveis" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex("/cadastro/imoveis$"));
        await Expect(page.Locator("#app-nav")).ToBeHiddenAsync();
    }
}