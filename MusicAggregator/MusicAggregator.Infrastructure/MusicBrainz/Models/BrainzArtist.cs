using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.MusicBrainz.Models
{
    internal sealed class BrainzArtist
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = "";

        [JsonPropertyName("score")]
        public int Score { get; init; }                

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("type")]
        public string? Type { get; init; }             

        [JsonPropertyName("country")]
        public string? Country { get; init; }         

        [JsonPropertyName("life-span")]
        public BrainzLifeSpan? LifeSpan { get; init; }

        [JsonPropertyName("tags")]
        public List<BrainzTag>? Tags { get; init; } 
    }
}
