using Microsoft.EntityFrameworkCore;

namespace ScanlationTracker.Infrastructure.Database.Entities;

[Index(nameof(ScanlationGroupId), nameof(ExternalId), IsUnique = true)]
public class SeriesEntity
{
    public required Guid Id { get; init; }

    public required Guid ScanlationGroupId { get; init; }

    public required string ExternalId { get; init; }

    public required string Title { get; init; }

    public required string RelativeCoverUrl { get; init; }

    public ScanlationGroupEntity? ScanlationGroup { get; init; }
}
