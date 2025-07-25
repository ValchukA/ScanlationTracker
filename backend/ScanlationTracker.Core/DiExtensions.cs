﻿using Microsoft.Extensions.DependencyInjection;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Core.Services;

namespace ScanlationTracker.Core;

public static class DiExtensions
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddSingleton<ISeriesService, SeriesService>();
        services.AddSingleton<CoreMetrics>();
    }
}
