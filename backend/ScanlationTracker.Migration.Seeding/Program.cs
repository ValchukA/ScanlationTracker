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
    var requiredUser = new UserEntity
    {
        Id = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86"),
        Username = "andva",
    };

    var userExists = await dbContext.Users.AnyAsync(user => user.Id == requiredUser.Id);

    if (!userExists)
    {
        dbContext.Users.Add(requiredUser);

        await dbContext.SaveChangesAsync();
    }
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
