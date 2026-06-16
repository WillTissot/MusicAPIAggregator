using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.Deezer.Models;

namespace MusicAggregator.Infrastructure.Deezer
{
    internal static class DeezerMapper
    {
        public static TrackInfo ToTrackInfo(this DeezerTrack t) => new()
        {
            Title = t.Title,
            DurationSeconds = t.Duration,
            PreviewUrl = t.Preview,
            AlbumTitle = t.Album.Title,
            CoverUrl = t.Album.CoverXl,
            ArtistName = t.Artist.Name,
            ExplicitLyrics = t.ExplicitLyrics,
        };
    }
}
