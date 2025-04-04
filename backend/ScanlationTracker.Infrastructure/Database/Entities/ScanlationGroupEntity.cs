using Microsoft.EntityFrameworkCore;

namespace ScanlationTracker.Infrastructure.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class ScanlationGroupEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string PublicName { get; set; }

    public required string BaseWebsiteUrl { get; set; }

    public required string BaseCoverUrl { get; set; }
}
