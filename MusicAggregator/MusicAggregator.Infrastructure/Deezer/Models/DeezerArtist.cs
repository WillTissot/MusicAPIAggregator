using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.Deezer.Models
{
    internal sealed class DeezerArtist
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("picture_xl")]
        public string? PictureXl { get; init; }
    }
}
