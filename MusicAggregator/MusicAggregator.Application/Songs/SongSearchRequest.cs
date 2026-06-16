

namespace MusicAggregator.Application.Songs
{
    public sealed class SongSearchRequest
    {
        public string Query { get; set; } = "";
        public int? TrackMinDuration { get; set; }
        public string? Country { get; set; }
        public bool SortByDuration { get; set; } = true;
    }
}
