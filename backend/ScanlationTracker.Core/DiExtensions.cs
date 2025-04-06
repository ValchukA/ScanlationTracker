using Microsoft.Extensions.DependencyInjection;
using ScanlationTracker.Core.Services;

namespace ScanlationTracker.Core;

public static class DiExtensions
{
    public static void AddCore(this IServiceCollection services) => services.AddScoped<ISeriesService, SeriesService>();
}
