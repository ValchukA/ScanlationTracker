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
}
