using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Songs
{
    internal class MusicAggregatorService : IMusicAggregatorService
    {
        private readonly ITrackProvider _trackProvider;
        private readonly IArtistProvider _artistProvider;
        private readonly ILyricsProvider _lyricsProvider;

        private readonly ILogger<MusicAggregatorService> _logger;

        public MusicAggregatorService(ITrackProvider trackProvider, IArtistProvider artistProvider, ILyricsProvider lyricsProvider, ILogger<MusicAggregatorService> logger)
        {
            _trackProvider = trackProvider;
            _artistProvider = artistProvider;
            _lyricsProvider = lyricsProvider;
            _logger = logger;
        }

        public async Task<SongPage> GetSongFullInfoAsync(string track, string artist, CancellationToken ct)
        {

            var trackTask = SafeCallAsync(() => _trackProvider.GetTrackAsync(artist, track, ct), "Track");
            var artistTask = SafeCallAsync(() => _artistProvider.GetArtistAsync(artist, ct), "Artist");
            var lyricsTask = SafeCallAsync(() => _lyricsProvider.GetLyricsInfoAsync(artist, track, ct), "Lyrics");

            await Task.WhenAll(trackTask, artistTask, lyricsTask);

            _logger.LogDebug("Fetching info for track, artist and lyrics is finished.");

            var trackResult = trackTask.Result;
            var artistResult = artistTask.Result;
            var lyricsResult = lyricsTask.Result;

            var warnings = new[] { trackResult.Warning, artistResult.Warning, lyricsResult.Warning }
                .Where(w => w is not null)
                .Select(w => w!)
                .ToArray();

            return new SongPage
            {
                Query = $"{artist} - {track}",
                Track = trackResult.Value,
                Artist = artistResult.Value,
                Lyrics = lyricsResult.Value,
                Sources = new SourceStatus
                {
                    Track = trackResult.Value is not null,
                    Artist = artistResult.Value is not null,
                    Lyrics = lyricsResult.Value is not null,
                    Warnings = warnings,
                },
            };
        }

        public async Task<SearchResult> SearchAsync(SongSearchRequest request, CancellationToken ct)
        {
            var tracksTask = SafeCallAsync(() => _trackProvider.SearchTracksAsync(request.Query, ct), "Tracks");
            var artistsTask = SafeCallAsync(() => _artistProvider.SearchArtistsAsync(request.Query, ct), "Artists");

            await Task.WhenAll(tracksTask, artistsTask);

            var tracksResult = tracksTask.Result;
            var artistsResult = artistsTask.Result;

            IEnumerable<TrackInfo> tracks = tracksResult.Value ?? [];
            if (request.TrackMinDuration is not null)
                tracks = tracks.Where(t => t.DurationSeconds >= request.TrackMinDuration);

            IEnumerable<ArtistInfo> artists = artistsResult.Value ?? [];
            if (!string.IsNullOrWhiteSpace(request.Country))
                artists = artists.Where(a =>
                    string.Equals(a.Country, request.Country, StringComparison.OrdinalIgnoreCase));

            if (request.SortByDuration)
            {
                tracks = tracks.OrderByDescending(t => t.DurationSeconds);
            }

            var warnings = new[] { tracksResult.Warning, artistsResult.Warning }
                .Where(w => w is not null)
                .Select(w => w!)
                .ToArray();

            return new SearchResult
            {
                Query = request.Query,
                Tracks = tracks.ToList(),     
                Artists = artists.ToList(),
                Sources = new SourceStatus
                {
                    Track = tracksResult.Value is not null,
                    Artist = artistsResult.Value is not null,
                    Lyrics = false,           
                    Warnings = warnings,
                },
            };
        }

        private async Task<ProviderResult<T>> SafeCallAsync<T>(Func<Task<T?>> call, string source) where T : class
        {
            try
            {
                return new ProviderResult<T>(await call(), Warning: null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "{Source} provider failed; degrading gracefully", source);
                return new ProviderResult<T>(Value: null, $"{source} provider unavailable; returned partial result.");
            }
        }

        private sealed record ProviderResult<T>(T? Value, string? Warning) where T : class;

    }
}
