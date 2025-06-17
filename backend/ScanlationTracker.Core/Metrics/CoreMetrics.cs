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

        _addedSeriesCounter =
            meter.CreateCounter<int>("scanlationtracker.series_update.series_added");
        _addedChaptersCounter =
            meter.CreateCounter<int>("scanlationtracker.series_update.chapters_added");

        _seriesUpdateDurationHistogram = meter.CreateHistogram(
            "scanlationtracker.series_update.duration",
            "s",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [3, 10, 30, double.PositiveInfinity],
            });
    }

    public static string MeterName { get; } = typeof(CoreMetrics).Assembly.GetName().Name!;

    public void IncrementAddedSeriesCounter() => _addedSeriesCounter.Add(1);

    public void IncrementAddedChaptersCounter() => _addedChaptersCounter.Add(1);

    public void AddSeriesUpdateDuration(TimeSpan duration)
        => _seriesUpdateDurationHistogram.Record(duration.TotalSeconds);
}
