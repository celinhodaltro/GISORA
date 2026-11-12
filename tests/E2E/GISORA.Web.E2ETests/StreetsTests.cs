using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class StreetsTests(WebAppFixture fixture)
{
    [Fact]
    public async Task CreatesStreetAndKeepsItAfterReload()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/streets");

        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua da Matriz");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();

        var row = page.GetByText("Rua da Matriz", new PageGetByTextOptions { Exact = true });
        await Expect(row).ToHaveCountAsync(1);

        await page.ReloadAsync();

        await Expect(page.GetByText("Rua da Matriz", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task RejectsDuplicatedStreetName()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/streets");

        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua Duplicada");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();
        await Expect(page.GetByText("Rua Duplicada", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        await page.GetByPlaceholder("Nome do logradouro").FillAsync("Rua Duplicada");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Cadastrar" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Ja existe logradouro com este nome.");
        await Expect(page.GetByText("Rua Duplicada", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
    }
}