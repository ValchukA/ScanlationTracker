using Microsoft.EntityFrameworkCore;

namespace ScanlationTracker.Infrastructure.Database.Entities;

[Index(nameof(SeriesId), nameof(ExternalId), IsUnique = true)]
[Index(nameof(SeriesId), nameof(Number), IsUnique = true)]
public class ChapterEntity
{
    public required Guid Id { get; init; }

    public required Guid SeriesId { get; init; }

    public required string ExternalId { get; init; }

    public required string Title { get; init; }

    public required int Number { get; init; }

    public required DateTimeOffset AddedAt { get; init; }

    public SeriesEntity? Series { get; init; }
}
