using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure.Scrapers.BrowserContext;

internal class PlaywrightSettings
{
    public const string SectionKey = "Playwright";

    [Required]
    public required string BrowserStoragePath { get; init; }

    [Required]
    public required string AdBlockExtensionPath { get; init; }

    [Range(15, 600)]
    public required int TimeoutInSeconds { get; init; }
}
