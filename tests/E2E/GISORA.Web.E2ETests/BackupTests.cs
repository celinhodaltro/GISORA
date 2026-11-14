using System.Text;
using System.Text.Json;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class BackupTests(WebAppFixture fixture)
{
    [Fact]
    public async Task ExportsClearsAndRestoresLocalData()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Carregar dados de demonstracao" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("Dados demonstrativos carregados");

        await page.GotoAsync($"{fixture.BaseUrl}/data");

        var download = await page.RunAndWaitForDownloadAsync(async () =>
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Exportar backup" }).ClickAsync());

        var backupPath = Path.Combine(Path.GetTempPath(), $"gisora-backup-{Guid.NewGuid():N}.json");
        await download.SaveAsAsync(backupPath);

        var json = await File.ReadAllTextAsync(backupPath);
        Assert.Contains("schemaVersion", json, StringComparison.Ordinal);
        Assert.Contains("gisora:properties", json, StringComparison.Ordinal);

        await page.EvaluateAsync("() => window.localStorage.clear()");
        await page.ReloadAsync();

        await page.Locator("input[type=file]").SetInputFilesAsync(backupPath);
        await Expect(page.Locator(".backup-validation")).ToContainTextAsync("Registros:");

        await page.Locator("select[aria-label='Modo de restauracao']").SelectOptionAsync("Replace");
        await page.Locator(".backup-check input").CheckAsync();
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restaurar backup" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Status)).ToContainTextAsync("registros restaurados");

        await page.GotoAsync(fixture.BaseUrl);
        await Expect(CardValue(page, "Imoveis")).ToHaveTextAsync("3");
    }

    [Fact]
    public async Task RejectsIncompatibleBackupVersion()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.BaseUrl}/data");

        var payload = JsonSerializer.Serialize(new
        {
            schemaVersion = 99,
            exportedAt = DateTimeOffset.UtcNow,
            records = new Dictionary<string, string> { ["gisora:meta:schema"] = "99" },
        });

        await page.Locator("input[type=file]").SetInputFilesAsync(new FilePayload
        {
            Name = "backup-incompativel.json",
            MimeType = "application/json",
            Buffer = Encoding.UTF8.GetBytes(payload),
        });

        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("incompativel");
        await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restaurar backup" })).ToBeDisabledAsync();
    }

    private static ILocator CardValue(IPage page, string label)
        => page.Locator(".gisora-stat-card", new PageLocatorOptions { HasTextString = label })
            .Locator(".gisora-stat-card__value");
}