using System.Diagnostics.Metrics;

namespace ScanlationTracker.Core.Metrics;

internal class CoreMetrics
{
    private readonly Histogram<double> _seriesUpdateDurationHistogram;

    public CoreMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _seriesUpdateDurationHistogram = meter.CreateHistogram(
            "scanlationtracker.series_update.duration",
            "s",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries = [10, 30, 60, double.PositiveInfinity],
            });
    }

    public static string MeterName { get; } = typeof(CoreMetrics).Assembly.GetName().Name!;

    public void AddSeriesUpdateDuration(TimeSpan duration)
        => _seriesUpdateDurationHistogram.Record(duration.TotalSeconds);
}
