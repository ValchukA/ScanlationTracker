using ScanlationTracker.Core;
using ScanlationTracker.Core.Repositories.Dtos;
using ScanlationTracker.Infrastructure.Database.Entities;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal static class MappingExtensions
{
    public static ScanlationGroupDto ToDto(this ScanlationGroupEntity entity)
    {
        var name = entity.Name switch
        {
            GroupNameConstants.AsuraScans => ScanlationGroupName.AsuraScans,
            GroupNameConstants.RizzFables => ScanlationGroupName.RizzFables,
            _ => throw new ArgumentException("Unexpected value"),
        };

        return new ScanlationGroupDto
        {
            Id = entity.Id,
            Name = name,
            PublicName = entity.PublicName,
            BaseWebsiteUrl = entity.BaseWebsiteUrl,
            BaseCoverUrl = entity.BaseCoverUrl,
        };
    }

    public static SeriesDto ToDto(this SeriesEntity entity) => new()
    {
        Id = entity.Id,
        ScanlationGroupId = entity.ScanlationGroupId,
        ExternalId = entity.ExternalId,
        Title = entity.Title,
        RelativeCoverUrl = entity.RelativeCoverUrl,
    };

    public static ChapterDto ToDto(this ChapterEntity entity) => new()
    {
        Id = entity.Id,
        SeriesId = entity.SeriesId,
        ExternalId = entity.ExternalId,
        Title = entity.Title,
        Number = entity.Number,
        AddedAt = entity.AddedAt,
    };

    public static SeriesEntity ToEntity(this SeriesDto dto) => new()
    {
        Id = dto.Id,
        ScanlationGroupId = dto.ScanlationGroupId,
        ExternalId = dto.ExternalId,
        Title = dto.Title,
        RelativeCoverUrl = dto.RelativeCoverUrl,
    };

    public static ChapterEntity ToEntity(this ChapterDto dto) => new()
    {
        Id = dto.Id,
        SeriesId = dto.SeriesId,
        ExternalId = dto.ExternalId,
        Title = dto.Title,
        Number = dto.Number,
        AddedAt = dto.AddedAt,
    };
}
