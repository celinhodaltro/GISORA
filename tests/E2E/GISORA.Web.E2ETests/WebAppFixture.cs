using System.Diagnostics;
using System.Text;

using Microsoft.Playwright;

namespace GISORA.Web.E2ETests;

public sealed class WebAppFixture : IAsyncLifetime
{
    private const int StartupTimeoutSeconds = 180;

    private readonly StringBuilder _hostLog = new();
    private Process? _host;
    private IPlaywright _playwright = null!;

    public string BaseUrl { get; } = $"http://127.0.0.1:{Random.Shared.Next(5300, 5900)}";

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        StartHost();
        await WaitForHostAsync();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_host is not null && _host.HasExited is false)
        {
            _host.Kill(entireProcessTree: true);
            await _host.WaitForExitAsync();
        }

        _host?.Dispose();
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        return page;
    }

    private void StartHost()
    {
        var projectPath = LocateWebProject();

        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = LocateRepositoryRoot(),
        };

        foreach (var argument in new[] { "run", "--project", projectPath, "-c", "Release", "--urls", BaseUrl })
        {
            startInfo.ArgumentList.Add(argument);
        }

        _host = Process.Start(startInfo) ?? throw new InvalidOperationException("Nao foi possivel iniciar o host web.");

        _host.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                _hostLog.AppendLine(args.Data);
            }
        };

        _host.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                _hostLog.AppendLine(args.Data);
            }
        };

        _host.BeginOutputReadLine();
        _host.BeginErrorReadLine();
    }

    private async Task WaitForHostAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var deadline = DateTime.UtcNow.AddSeconds(StartupTimeoutSeconds);
        var lastError = string.Empty;

        while (DateTime.UtcNow < deadline)
        {
            if (_host!.HasExited)
            {
                throw new InvalidOperationException($"Host web encerrou durante o startup. {_hostLog}");
            }

            try
            {
                var response = await client.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                lastError = $"HTTP {(int)response.StatusCode}";
            }
            catch (HttpRequestException exception)
            {
                lastError = exception.Message;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException($"Host web nao respondeu em {StartupTimeoutSeconds}s. Ultimo erro: {lastError}. Saida do host: {_hostLog}");
    }

    private static string LocateWebProject()
        => Path.Combine(LocateRepositoryRoot(), "src", "presentations", "GISORA.Presentation", "GISORA.Presentation.csproj");

    private static string LocateRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && File.Exists(Path.Combine(directory.FullName, "GISORA.slnx")) is false)
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("GISORA.slnx nao encontrado a partir do diretorio de teste.");
        }

        return directory.FullName;
    }
}

[CollectionDefinition(Name)]
public sealed class WebAppGroup : ICollectionFixture<WebAppFixture>
{
    public const string Name = "webapp";
}