using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Infrastructure.Database;
using ScanlationTracker.Infrastructure.Database.Repositories;
using ScanlationTracker.Infrastructure.Scrapers;
using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure;

public static class DiExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgreSql(configuration);
        services.AddScoped<ISeriesRepository, SeriesEfRepository>();
        services.AddSingleton<IScanlationScraperFactory, ScanlationScraperFactory>();
    }

    public static void AddPostgreSql(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(PostgreSqlSettings.SectionKey).Get<PostgreSqlSettings>()!;
        Validator.ValidateObject(settings, new ValidationContext(settings));

        services.AddDbContext<ScanlationDbContext>(options => options.UseNpgsql(settings.ConnectionString));
    }
}
