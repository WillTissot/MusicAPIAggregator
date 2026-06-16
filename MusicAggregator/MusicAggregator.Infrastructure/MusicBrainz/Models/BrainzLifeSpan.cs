using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.MusicBrainz.Models
{
    internal sealed class BrainzLifeSpan
    {
        [JsonPropertyName("begin")]
        public string? Begin { get; init; }   

        [JsonPropertyName("ended")]
        public bool? Ended { get; init; }
    }
}
