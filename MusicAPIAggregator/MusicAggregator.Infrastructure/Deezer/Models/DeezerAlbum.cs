using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.Deezer.Models
{
    internal sealed class DeezerAlbum
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = "";

        [JsonPropertyName("cover_xl")]
        public string? CoverXl { get; init; }
    }
}
