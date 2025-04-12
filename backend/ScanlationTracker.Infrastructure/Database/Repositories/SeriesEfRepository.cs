using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Dtos;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal class SeriesEfRepository : ISeriesRepository
{
    private readonly ScanlationDbContext _dbContext;

    public SeriesEfRepository(ScanlationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ScanlationGroupDto[]> GetAllGroupsAsync()
        => await _dbContext.ScanlationGroups.Select(group => group.ToDto()).AsNoTracking().ToArrayAsync();
}
