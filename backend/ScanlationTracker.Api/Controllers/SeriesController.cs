using Microsoft.AspNetCore.Mvc;
using ScanlationTracker.Api.Contracts;
using ScanlationTracker.Core.Services.Interfaces;

namespace ScanlationTracker.Api.Controllers;

[ApiController]
[Route("series")]
public class SeriesController : ControllerBase
{
    private const string _trackingsRoutePart = "trackings";

    private readonly ISeriesService _seriesService;
    private readonly Guid _userId = Guid.Parse("01980e47-61e7-736d-b7d9-5c0a4121ad86");

    public SeriesController(ISeriesService seriesService) => _seriesService = seriesService;

    [HttpPost($"{{{nameof(seriesId)}}}/{_trackingsRoutePart}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateTrackingResponse>> CreateTrackingAsync(Guid seriesId)
    {
        var result = await _seriesService.CreateTrackingAsync(seriesId, _userId);

        return result.Match(
            GetSeriesCreatedResponse,
            seriesNotFoundError => Problem(
                $"Series with Id {seriesId} does not exist",
                statusCode: StatusCodes.Status404NotFound),
            duplicateTrackingError => Problem(
                $"Tracking for series with Id {seriesId} already exists",
                statusCode: StatusCodes.Status409Conflict));

        CreatedResult GetSeriesCreatedResponse(Guid trackingId)
        {
            var response = new CreateTrackingResponse { TrackingId = trackingId };

            return Created((string?)null, response);
        }
    }

    [HttpDelete($"{_trackingsRoutePart}/{{{nameof(trackingId)}}}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTrackingAsync(Guid trackingId)
    {
        var result = await _seriesService.DeleteTrackingAsync(trackingId, _userId);

        return result.Match<ActionResult>(
            success => NoContent(),
            trackingNotFoundError => Problem(
                $"Tracking with Id {trackingId} does not exist",
                statusCode: StatusCodes.Status404NotFound),
            unauthorizedError => Problem(statusCode: StatusCodes.Status403Forbidden));
    }
}
