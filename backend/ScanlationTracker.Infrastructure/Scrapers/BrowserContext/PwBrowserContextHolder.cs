using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

internal class PwBrowserContextHolder : IPwBrowserContextHolder
{
    private readonly PlaywrightSettings _settings;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<PwBrowserContextHolder> _logger;
    private IBrowserContext? _context;

    public PwBrowserContextHolder(IOptions<PlaywrightSettings> settings, ILogger<PwBrowserContextHolder> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IBrowserContext> GetContextAsync()
    {
        await EnsureBrowserInitializedAsync();

        return _context!;
    }

    private async Task EnsureBrowserInitializedAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_context is null)
            {
                var playwright = await Playwright.CreateAsync();
                _context = await playwright.Chromium.LaunchPersistentContextAsync(
                    _settings.BrowserStoragePath,
                    new()
                    {
                        Channel = "chromium",
                        ChromiumSandbox = true,
                        UserAgent = playwright.Devices["Desktop Chrome"].UserAgent,
                        Args =
                        [
                            $"--disable-extensions-except={_settings.AdBlockExtensionPath}",
                            "--blink-settings=imagesEnabled=false",
                        ],
                    });

                var timeout = TimeSpan.FromSeconds(_settings.TimeoutInSeconds).TotalMilliseconds;
                _context.SetDefaultTimeout((float)timeout);

                _context.RequestFailed += (sender, request) =>
                {
                    if (request.Failure == "net::ERR_TIMED_OUT")
                    {
                        _logger.LogWarning("{Error}", request.Failure);
                    }
                };
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
