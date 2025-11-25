using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Scalar.AspNetCore;
using ScanlationTracker.Core;
using ScanlationTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var secretsDirectoryPath = builder.Configuration.GetValue<string>("SecretsDirectory");

if (!string.IsNullOrEmpty(secretsDirectoryPath))
{
    builder.Configuration.AddKeyPerFile(secretsDirectoryPath);
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCore();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
        resourceBuilder.AddService(builder.Environment.ApplicationName))
    .UseOtlpExporter()
    .WithMetrics(providerBuilder => providerBuilder.AddAspNetCoreInstrumentation());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.MapControllers();

await app.RunAsync();
