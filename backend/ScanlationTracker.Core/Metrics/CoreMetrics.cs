using System.Diagnostics.Metrics;

namespace ScanlationTracker.Core.Metrics;

internal class CoreMetrics
{
    private readonly Counter<int> _addedSeriesCounter;
    private readonly Counter<int> _addedChaptersCounter;
    private readonly Histogram<double> _seriesUpdateDurationHistogram;

    public CoreMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        var seriesUpdatePrefix = "scanlationtracker.series_update";

        _addedSeriesCounter = meter.CreateCounter<int>($"{seriesUpdatePrefix}.series_added");
        _addedChaptersCounter = meter.CreateCounter<int>($"{seriesUpdatePrefix}.chapters_added");
        _seriesUpdateDurationHistogram = meter.CreateHistogram<double>(
            $"{seriesUpdatePrefix}.duration",
            "s",
            advice: new() { HistogramBucketBoundaries = [5, 15, 60, 300, 600] });
    }

    public static string MeterName { get; } = typeof(CoreMetrics).Assembly.GetName().Name!;

    public void IncrementAddedSeriesCounter(ScanlationGroupName groupName)
        => _addedSeriesCounter.Add(1, CreateGroupNameTag(groupName));

    public void IncrementAddedChaptersCounter(ScanlationGroupName groupName)
        => _addedChaptersCounter.Add(1, CreateGroupNameTag(groupName));

    public void AddSeriesUpdateDuration(TimeSpan duration)
        => _seriesUpdateDurationHistogram.Record(duration.TotalSeconds);

    private static KeyValuePair<string, object?> CreateGroupNameTag(ScanlationGroupName groupName)
        => new("scanlation_group.name", groupName);
}
