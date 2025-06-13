using OpenTelemetry.Metrics;

namespace ScanlationTracker.Core.Metrics;

public static class MeterProviderBuilderExtensions
{
    public static void AddCoreMeter(this MeterProviderBuilder meterProviderBuilder)
        => meterProviderBuilder.AddMeter(CoreMetrics.MeterName);
}
