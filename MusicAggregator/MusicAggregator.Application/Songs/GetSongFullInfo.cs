using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Songs
{
    internal class GetSongFullInfo : IGetSongFullInfo
    {
        private readonly ITrackProvider _trackProvider;
        private readonly IArtistProvider _artistProvider;
        private readonly ILyricsProvider _lyricsProvider;

        private readonly ILogger<GetSongFullInfo> _logger;

        public GetSongFullInfo(ITrackProvider trackProvider, IArtistProvider artistProvider, ILyricsProvider lyricsProvider, ILogger<GetSongFullInfo> logger)
        {
            _trackProvider = trackProvider;
            _artistProvider = artistProvider;
            _lyricsProvider = lyricsProvider;
            _logger = logger;
        }

        public async Task<SongPage> GetSongFullInfoAsync(string track, string artist, CancellationToken ct)
        {

            Task<TrackInfo?> trackTask = _trackProvider.SearchTrackAsync(track, artist, ct);
            Task<ArtistInfo?> artistTask = _artistProvider.GetArtistAsync(artist, ct);
            Task<LyricsInfo?> lyricsTask = _lyricsProvider.GetLyricsInfoAsync(track, artist, ct);

            await Task.WhenAll(trackTask, artistTask, lyricsTask);

            _logger.LogDebug("Fetching info for track, artist and lyrics is finished.");

            TrackInfo? trackInfo = trackTask.Result;
            ArtistInfo? artistInfo = artistTask.Result;
            LyricsInfo? lyricsInfo = lyricsTask.Result;

            SongPage songPage = new()
            {
                Query = $"Info for track:{track} and artist: {artist}",
                Sources = new()
                {
                    Artist = artistInfo is not null,
                    Lyrics = lyricsInfo is not null,
                    Track = trackInfo is not null
                },
                Artist = artistInfo,
                Lyrics = lyricsInfo,
                Track = trackInfo

            };

            return songPage;
        }
    }
}
