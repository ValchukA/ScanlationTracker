using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScanlationTracker.Infrastructure;
using ScanlationTracker.Infrastructure.Database;
using ScanlationTracker.Infrastructure.Database.Entities;

var dbContext = GetDbContext();

await dbContext.Database.MigrateAsync();
await SeedUsersAsync();
await SeedScanlationGroupsAsync();

ScanlationDbContext GetDbContext()
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddPostgreSql(builder.Configuration);
    var host = builder.Build();

    return host.Services.GetRequiredService<ScanlationDbContext>();
}

async Task SeedUsersAsync()
{
    var requiredUsernames = new[] { "andva" };
    var existingUsernames = await dbContext.Users.Select(user => user.Username).ToHashSetAsync();

    foreach (var requiredUsername in requiredUsernames)
    {
        if (!existingUsernames.Contains(requiredUsername))
        {
            dbContext.Users.Add(new UserEntity { Id = Guid.CreateVersion7(), Username = requiredUsername });
        }
    }

    await dbContext.SaveChangesAsync();
}

async Task SeedScanlationGroupsAsync()
{
    var requiredGroups = new[]
    {
        new
        {
            Name = GroupNameConstants.AsuraScans,
            PublicName = "Asura Scans",
            BaseWebsiteUrl = "https://asuracomic.net/",
            BaseCoverUrl = "https://gg.asuracomic.net/",
        },
        new
        {
            Name = GroupNameConstants.RizzFables,
            PublicName = "Rizz Fables",
            BaseWebsiteUrl = "https://rizzfables.com/",
            BaseCoverUrl = "https://rizzfables.com/",
        },
    };

    var existingGroups = await dbContext.ScanlationGroups.ToDictionaryAsync(group => group.Name);

    foreach (var requiredGroup in requiredGroups)
    {
        if (existingGroups.TryGetValue(requiredGroup.Name, out var existingGroup))
        {
            existingGroup.PublicName = requiredGroup.PublicName;
            existingGroup.BaseWebsiteUrl = requiredGroup.BaseWebsiteUrl;
            existingGroup.BaseCoverUrl = requiredGroup.BaseCoverUrl;
        }
        else
        {
            dbContext.Add(new ScanlationGroupEntity
            {
                Id = Guid.CreateVersion7(),
                Name = requiredGroup.Name,
                PublicName = requiredGroup.PublicName,
                BaseWebsiteUrl = requiredGroup.BaseWebsiteUrl,
                BaseCoverUrl = requiredGroup.BaseCoverUrl,
            });
        }
    }

    await dbContext.SaveChangesAsync();
}
