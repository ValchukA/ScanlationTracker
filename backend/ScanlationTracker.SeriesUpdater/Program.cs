using OpenTelemetry;
using OpenTelemetry.Resources;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Infrastructure;
using ScanlationTracker.SeriesUpdater;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<SeriesUpdaterWorker>();
builder.Services.AddCore();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
        resourceBuilder.AddService(builder.Environment.ApplicationName))
    .UseOtlpExporter()
    .WithMetrics(provideBuilder => provideBuilder.AddCoreMeter());

builder.Services.AddOptions<SeriesUpdaterSettings>()
    .BindConfiguration(SeriesUpdaterSettings.SectionKey).ValidateDataAnnotations();

var host = builder.Build();
await host.RunAsync();
