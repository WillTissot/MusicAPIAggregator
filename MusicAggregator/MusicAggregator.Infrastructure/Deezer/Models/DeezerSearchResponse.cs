using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.Deezer.Models
{
    internal sealed class DeezerSearchResponse
    {
        [JsonPropertyName("data")]
        public List<DeezerTrack> Data { get; init; } = [];

        [JsonPropertyName("total")]
        public int Total { get; init; }
    }
}
