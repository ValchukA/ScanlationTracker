using Microsoft.EntityFrameworkCore;
using ScanlationTracker.Core.Repositories;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal class SeriesEfRepositoryFactory : ISeriesRepositoryFactory
{
    private readonly IDbContextFactory<ScanlationDbContext> _dbContextFactory;

    public SeriesEfRepositoryFactory(IDbContextFactory<ScanlationDbContext> dbContextFactory)
        => _dbContextFactory = dbContextFactory;

    public ISeriesRepository CreateRepository()
        => new SeriesEfRepository(_dbContextFactory.CreateDbContext());
}
