using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicAggregator.Application.Models;
using MusicAggregator.Application.Songs;

namespace MusicAggregator.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MusicAggregatorController : ControllerBase
    {
        private readonly IMusicAggregatorService _aggregatorService;

        public MusicAggregatorController(IMusicAggregatorService aggregatorService)
        {
            _aggregatorService = aggregatorService;
        }

        [HttpGet("song")]
        [ProducesResponseType(typeof(SongPage), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SongPage>> GetSong([FromQuery] string artist, [FromQuery] string track, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(track))
                return BadRequest("Both 'artist' and 'track' query parameters are required.");

            var result = await _aggregatorService.GetSongFullInfoAsync(track, artist, ct);

            if (result.Track is null && result.Artist is null && result.Lyrics is null)
                return NotFound($"No data found for '{artist} - {track}'.");

            return Ok(result); 
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(SongPage), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SongPage>> SearchSongs([FromQuery] SongSearchRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query parameter is required!");

            var result = await _aggregatorService.SearchAsync(request, ct);

            return Ok(result); 
        }
    }
}
