using ScanlationTracker.Core;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Infrastructure.Database.Entities;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal static class MappingExtensions
{
    public static ScanlationGroup ToModel(this ScanlationGroupEntity entity)
    {
        var name = entity.Name switch
        {
            GroupNameConstants.AsuraScans => ScanlationGroupName.AsuraScans,
            GroupNameConstants.RizzFables => ScanlationGroupName.RizzFables,
            _ => throw new ArgumentException("Unexpected value"),
        };

        return new ScanlationGroup
        {
            Id = entity.Id,
            Name = name,
            PublicName = entity.PublicName,
            BaseWebsiteUrl = entity.BaseWebsiteUrl,
            BaseCoverUrl = entity.BaseCoverUrl,
        };
    }

    public static Series ToModel(this SeriesEntity entity) => new()
    {
        Id = entity.Id,
        ScanlationGroupId = entity.ScanlationGroupId,
        ExternalId = entity.ExternalId,
        Title = entity.Title,
        RelativeCoverUrl = entity.RelativeCoverUrl,
    };

    public static Chapter ToModel(this ChapterEntity entity) => new()
    {
        Id = entity.Id,
        SeriesId = entity.SeriesId,
        ExternalId = entity.ExternalId,
        Title = entity.Title,
        Number = entity.Number,
        AddedAt = entity.AddedAt,
    };

    public static SeriesEntity ToEntity(this Series model) => new()
    {
        Id = model.Id,
        ScanlationGroupId = model.ScanlationGroupId,
        ExternalId = model.ExternalId,
        Title = model.Title,
        RelativeCoverUrl = model.RelativeCoverUrl,
    };

    public static ChapterEntity ToEntity(this Chapter model) => new()
    {
        Id = model.Id,
        SeriesId = model.SeriesId,
        ExternalId = model.ExternalId,
        Title = model.Title,
        Number = model.Number,
        AddedAt = model.AddedAt,
    };
}
