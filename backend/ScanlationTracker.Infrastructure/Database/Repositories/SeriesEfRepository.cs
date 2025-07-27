using Microsoft.EntityFrameworkCore;
using Npgsql;
using ScanlationTracker.Core.Models;
using ScanlationTracker.Core.Repositories;
using ScanlationTracker.Core.Repositories.Exceptions;
using ScanlationTracker.Infrastructure.Database.Entities;

namespace ScanlationTracker.Infrastructure.Database.Repositories;

internal class SeriesEfRepository : ISeriesRepository
{
    private readonly ScanlationDbContext _dbContext;

    public SeriesEfRepository(ScanlationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ScanlationGroup[]> GetAllGroupsAsync()
    {
        var groupEntities = await _dbContext.ScanlationGroups.AsNoTracking().ToArrayAsync();

        return [.. groupEntities.Select(group => group.ToModel())];
    }

    public async Task<Series?> GetSeriesByExternalIdAsync(Guid groupId, string seriesExternalId)
    {
        var seriesEntity = await _dbContext.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(series => series.ScanlationGroupId == groupId
                && series.ExternalId == seriesExternalId);

        return seriesEntity?.ToModel();
    }

    public async Task<Series[]> GetSeriesByTitleAsync(Guid groupId, string seriesTitle)
    {
        var seriesEntities = await _dbContext.Series
            .AsNoTracking()
            .Where(series => series.ScanlationGroupId == groupId && series.Title == seriesTitle)
            .ToArrayAsync();

        return [.. seriesEntities.Select(series => series.ToModel())];
    }

    public async Task<Chapter?> GetLatestChapterAsync(Guid seriesId)
    {
        var chapterEntity = await _dbContext.Chapters
            .AsNoTracking()
            .OrderBy(chapter => chapter.Number)
            .LastOrDefaultAsync(chapter => chapter.SeriesId == seriesId);

        return chapterEntity?.ToModel();
    }

    public async Task<SeriesTracking?> GetTrackingAsync(Guid trackingId)
    {
        var trackingEntity = await _dbContext.SeriesTrackings
            .AsNoTracking()
            .FirstOrDefaultAsync(tracking => tracking.Id == trackingId);

        return trackingEntity?.ToModel();
    }

    public void AddSeries(Series series) => _dbContext.Series.Add(series.ToEntity());

    public void AddChapter(Chapter chapter) => _dbContext.Chapters.Add(chapter.ToEntity());

    public void AddTracking(SeriesTracking tracking)
        => _dbContext.SeriesTrackings.Add(tracking.ToEntity());

    public void UpdateSeries(Series series) => _dbContext.Series.Update(series.ToEntity());

    public void DeleteTracking(Guid trackingId)
        => _dbContext.SeriesTrackings.Remove(new SeriesTrackingEntity { Id = trackingId });

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException)
        {
            switch (postgresException.SqlState)
            {
                case PostgresErrorCodes.ForeignKeyViolation:
                    throw new ForeignKeyConstraintException(exception);
                case PostgresErrorCodes.UniqueViolation:
                    throw new UniqueConstraintException(exception);
                default:
                    throw;
            }
        }
    }
}
