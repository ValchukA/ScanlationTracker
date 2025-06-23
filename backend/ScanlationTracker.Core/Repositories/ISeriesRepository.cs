using ScanlationTracker.Core.Repositories.Dtos;

namespace ScanlationTracker.Core.Repositories;

public interface ISeriesRepository
{
    public Task<ScanlationGroupDto[]> GetAllGroupsAsync();

    public Task<SeriesDto?> GetSeriesByExternalIdAsync(Guid groupId, string seriesExternalId);

    public Task<SeriesDto?> GetSeriesByTitleAsync(Guid groupId, string seriesTitle);

    public Task<ChapterDto?> GetLatestChapterAsync(Guid seriesId);

    public void AddSeries(SeriesDto series);

    public void UpdateSeries(SeriesDto series);

    public void AddChapter(ChapterDto chapter);

    public Task SaveChangesAsync();
}
