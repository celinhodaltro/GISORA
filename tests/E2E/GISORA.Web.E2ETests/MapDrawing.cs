using Microsoft.Playwright;

namespace GISORA.Web.E2ETests;

internal static class MapDrawing
{
    public const string Pencil = "Desenhar no mapa";

    public const string Finish = "Concluir desenho";

    public const string Cancel = "Cancelar desenho";

    public const string Undo = "Desfazer ultimo vertice";

    public static async Task StartAsync(IPage page, string? mode = null)
    {
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = Pencil }).ClickAsync();

        if (mode is not null)
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = mode, Exact = true }).ClickAsync();
        }
    }

    public static Task FinishAsync(IPage page)
        => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = Finish }).ClickAsync();

    public static Task CancelAsync(IPage page)
        => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = Cancel }).ClickAsync();
}
