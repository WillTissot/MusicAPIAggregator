using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.LRCLIB.Models
{
    internal sealed class LrclibResponse
    {
        [JsonPropertyName("instrumental")]
        public bool Instrumental { get; init; }

        [JsonPropertyName("plainLyrics")]
        public string? PlainLyrics { get; init; }

        [JsonPropertyName("syncedLyrics")]
        public string? SyncedLyrics { get; init; }
    }
}
