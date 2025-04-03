using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanlationTracker.Infrastructure.Database;
using System.ComponentModel.DataAnnotations;

namespace ScanlationTracker.Infrastructure;

public static class DiExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services.AddPostgreSql(configuration);

    public static void AddPostgreSql(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection(PostgreSqlSettings.SectionKey).Get<PostgreSqlSettings>()!;
        Validator.ValidateObject(settings, new ValidationContext(settings));

        services.AddDbContext<ScanlationDbContext>(options => options.UseNpgsql(settings.ConnectionString));
    }
}
