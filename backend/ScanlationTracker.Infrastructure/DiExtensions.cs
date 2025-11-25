using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Scrapers;
using ScanlationTracker.Core.UrlManagers;
using ScanlationTracker.Infrastructure.Database;
using ScanlationTracker.Infrastructure.Database.Repositories;
using ScanlationTracker.Infrastructure.Scrapers;
using ScanlationTracker.Infrastructure.Scrapers.BrowserContext;
using ScanlationTracker.Infrastructure.UrlManagers;
using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure;

public static class DiExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgreSql(configuration);
        services.AddScoped<ISeriesRepository, SeriesEfRepository>();
        services.AddSingleton<ISeriesRepositoryFactory, SeriesEfRepositoryFactory>();
        services.AddSingleton<IPwBrowserContextHolder, PwBrowserContextHolder>();
        services.AddSingleton<IUrlManagerFactory, UrlManagerFactory>();
        services.AddSingleton<IScanlationScraperFactory, ScanlationScraperFactory>();
        services.AddOptions<PlaywrightSettings>()
            .BindConfiguration(PlaywrightSettings.SectionKey)
            .ValidateDataAnnotations();
    }

    public static void AddPostgreSql(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetRequiredSection(PostgreSqlSettings.SectionKey).Get<PostgreSqlSettings>()!;
        Validator.ValidateObject(settings, new ValidationContext(settings));

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = settings.Host,
            Username = settings.Username,
            Password = settings.Password,
            Database = settings.Database,
        };

        services.AddDbContextFactory<ScanlationDbContext>(options =>
            options.UseNpgsql(connectionStringBuilder.ConnectionString));
    }
}
