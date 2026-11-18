using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class AuthenticationTests(WebAppFixture fixture)
{
    private const float ColdStartTimeout = 30_000;

    [Fact]
    public async Task RedirectsAnonymousVisitorToLogin()
    {
        var page = await fixture.NewAnonymousPageAsync();

        await page.GotoAsync(fixture.BaseUrl);

        await Expect(page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Acessar o GISORA" }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = ColdStartTimeout });
        await Expect(page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/login"),
            new PageAssertionsToHaveURLOptions { Timeout = ColdStartTimeout });
    }

    [Fact]
    public async Task RejectsInvalidCredentials()
    {
        var page = await fixture.NewAnonymousPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/login");

        await page.GetByLabel("E-mail").FillAsync("admin@gisora.gov.br");
        await page.GetByLabel("Senha").FillAsync("senha-incorreta");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Entrar" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("E-mail ou senha invalidos.");
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/login"));
    }

    [Fact]
    public async Task SignsInAndShowsSessionUserInSidebarFooter()
    {
        var page = await fixture.NewAnonymousPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/login");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Usar credenciais do administrador" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Entrar" }).ClickAsync();

        await Expect(page.Locator(".app-nav__account"))
            .ToContainTextAsync("Administrador", new LocatorAssertionsToContainTextOptions { Timeout = ColdStartTimeout });
        await Expect(page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Painel territorial" }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = ColdStartTimeout });

        var cookies = await page.Context.CookiesAsync();
        Assert.Contains(cookies, cookie => cookie.Name == "gisora_session" && cookie.Value.Contains('.'));
    }

    [Fact]
    public async Task PublicViewerCannotOpenEditingPages()
    {
        var page = await fixture.NewAnonymousPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/login");

        await page.GetByLabel("E-mail").FillAsync("consulta@gisora.gov.br");
        await page.GetByLabel("Senha").FillAsync("Gisora@2026");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Entrar" }).ClickAsync();

        await Expect(page.Locator(".app-nav__account"))
            .ToContainTextAsync("Consulta publica", new LocatorAssertionsToContainTextOptions { Timeout = ColdStartTimeout });
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cadastro no mapa" })).ToHaveCountAsync(0);
        await Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Usuarios e perfis" })).ToHaveCountAsync(0);

        await page.GotoAsync($"{fixture.BaseUrl}/cadastro/quadras");

        await Expect(page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Acesso negado" }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = ColdStartTimeout });
    }

    [Fact]
    public async Task AdministratorCreatesUserFromUsersPage()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/users");

        await page.GetByLabel("Nome", new PageGetByLabelOptions { Exact = true }).FillAsync("Maria Analista");
        await page.GetByLabel("E-mail", new PageGetByLabelOptions { Exact = true }).FillAsync("maria@gisora.gov.br");
        await page.GetByLabel("Senha inicial").FillAsync("Senha@2026");
        await page.GetByLabel("Perfil", new PageGetByLabelOptions { Exact = true }).SelectOptionAsync("Analyst");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Criar usuario" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Usuario criado.");
        await Expect(page.Locator(".users-table")).ToContainTextAsync("maria@gisora.gov.br");
    }
}