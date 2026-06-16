
namespace MusicAggregator.Application.Models
{
    public sealed class SongPage
    {
        public TrackInfo? Track { get; init; }
        public ArtistInfo? Artist { get; init; }
        public LyricsInfo? Lyrics { get; init; }

        public required string Query { get; init; }
        public required SourceStatus Sources { get; init; }
    }
}
