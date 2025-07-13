using ScanlationTracker.Core.Models;

namespace ScanlationTracker.Core.Repositories;

public interface ISeriesRepository
{
    public Task<ScanlationGroup[]> GetAllGroupsAsync();

    public Task<Series?> GetSeriesByExternalIdAsync(Guid groupId, string seriesExternalId);

    public Task<Series?> GetSeriesByTitleAsync(Guid groupId, string seriesTitle);

    public Task<Chapter?> GetLatestChapterAsync(Guid seriesId);

    public void AddSeries(Series series);

    public void UpdateSeries(Series series);

    public void AddChapter(Chapter chapter);

    public Task SaveChangesAsync();
}
