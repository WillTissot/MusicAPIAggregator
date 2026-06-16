using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.MusicBrainz.Models
{
    internal sealed class BrainzTag
    {
        [JsonPropertyName("count")]
        public int Count { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";
    }
}
