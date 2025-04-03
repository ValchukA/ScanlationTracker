namespace ScanlationTracker.Infrastructure.Database.Entities;

public class ScanlationGroup
{
    public required Guid Id { get; init; }

    public required ScanlationGroupNames Name { get; init; }

    public required string PublicName { get; set; }

    public required string BaseWebsiteUrl { get; set; }

    public required string BaseCoverUrl { get; set; }
}
