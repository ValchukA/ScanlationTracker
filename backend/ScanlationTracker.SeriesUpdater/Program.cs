using OpenTelemetry;
using OpenTelemetry.Resources;
using ScanlationTracker.Core;
using ScanlationTracker.Core.Metrics;
using ScanlationTracker.Infrastructure;
using ScanlationTracker.SeriesUpdater;

var builder = Host.CreateApplicationBuilder(args);

var secretsDirectoryPath = builder.Configuration.GetValue<string>("SecretsDirectory");

if (!string.IsNullOrEmpty(secretsDirectoryPath))
{
    builder.Configuration.AddKeyPerFile(secretsDirectoryPath);
}

builder.Services.AddHostedService<SeriesUpdaterWorker>();
builder.Services.AddCore();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
        resourceBuilder.AddService(builder.Environment.ApplicationName))
    .UseOtlpExporter()
    .WithMetrics(providerBuilder => providerBuilder.AddSeriesUpdaterMeter());

builder.Services.AddOptions<SeriesUpdaterSettings>()
    .BindConfiguration(SeriesUpdaterSettings.SectionKey).ValidateDataAnnotations();

var host = builder.Build();
await host.RunAsync();
