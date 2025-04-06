using ScanlationTracker.Core.Repositories.Dtos;

namespace ScanlationTracker.Core.Repositories;

public interface ISeriesRepository
{
    public Task<ScanlationGroupDto[]> GetAllGroupsAsync();
}
