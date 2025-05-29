using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

internal class PwBrowserContextHolder : IPwBrowserContextHolder
{
    private readonly PlaywrightSettings _settings;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private IBrowserContext? _context;

    public PwBrowserContextHolder(IOptions<PlaywrightSettings> settings) => _settings = settings.Value;

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
                        UserAgent = playwright.Devices["Desktop Chrome"].UserAgent,
                        Args =
                        [
                            $"--disable-extensions-except={_settings.AdBlockExtensionPath}",
                            "--blink-settings=imagesEnabled=false",
                        ],
                    });
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
