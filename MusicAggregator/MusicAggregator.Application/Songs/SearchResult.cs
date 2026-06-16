using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Songs
{
    public sealed class SearchResult
    {
        public required string Query { get; init; }
        public IReadOnlyList<TrackInfo> Tracks { get; init; } = [];
        public IReadOnlyList<ArtistInfo> Artists { get; init; } = [];
        public required SourceStatus Sources { get; init; }  
    }
}
