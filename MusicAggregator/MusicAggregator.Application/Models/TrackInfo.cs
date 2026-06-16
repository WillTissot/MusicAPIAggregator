namespace MusicAggregator.Application.Models
{
    public sealed class TrackInfo
    {
        public required string Title { get; init; }
        public int DurationSeconds { get; init; }
        public int Rank { get; init; }
        public string? PreviewUrl { get; init; }
        public string? AlbumTitle { get; init; }
        public string? CoverUrl { get; init; }
        public string? ArtistName { get; init; }
        public bool ExplicitLyrics { get; init; }
    }
}
