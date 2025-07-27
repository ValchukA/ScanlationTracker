using Microsoft.Extensions.DependencyInjection;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Core.Services;
using ScanlationTracker.Core.Services.Interfaces;

namespace ScanlationTracker.Core;

public static class DiExtensions
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddScoped<ISeriesService, SeriesService>();
        services.AddSingleton<ISeriesUpdaterService, SeriesUpdaterService>();
        services.AddSingleton<CoreMetrics>();
    }
}
