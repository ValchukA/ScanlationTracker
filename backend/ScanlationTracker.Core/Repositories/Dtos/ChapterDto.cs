namespace ScanlationTracker.Core.Repositories.Dtos;

public record ChapterDto
{
    public required Guid Id { get; init; }

    public required Guid SeriesId { get; init; }

    public required string ExternalId { get; init; }

    public required string Title { get; init; }

    public required int Number { get; init; }

    public required DateTimeOffset AddedAt { get; init; }
}
