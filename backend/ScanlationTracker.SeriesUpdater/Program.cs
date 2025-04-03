using ScanlationTracker.Infrastructure;
using ScanlationTracker.SeriesUpdater;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
