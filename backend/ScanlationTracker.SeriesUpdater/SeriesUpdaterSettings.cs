using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.SeriesUpdater;

internal class SeriesUpdaterSettings
{
    public const string SectionKey = "SeriesUpdater";

    [Range(1, 60 * 24)]
    public required int UpdateIntervalInMinutes { get; init; }
}
