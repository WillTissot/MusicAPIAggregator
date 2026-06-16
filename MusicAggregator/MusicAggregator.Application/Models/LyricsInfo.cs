
namespace MusicAggregator.Application.Models
{
    public sealed class LyricsInfo
    {
        public string? PlainLyrics { get; init; }
        public string? SyncedLyrics { get; init; } 
        public bool IsInstrumental { get; init; }
    }
}
