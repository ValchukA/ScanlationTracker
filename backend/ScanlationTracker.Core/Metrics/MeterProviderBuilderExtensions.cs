using OpenTelemetry.Metrics;

namespace ScanlationTracker.Core.Metrics;

public static class MeterProviderBuilderExtensions
{
    public static void AddSeriesUpdaterMeter(this MeterProviderBuilder meterProviderBuilder)
        => meterProviderBuilder.AddMeter(SeriesUpdaterMetrics.MeterName);
}
