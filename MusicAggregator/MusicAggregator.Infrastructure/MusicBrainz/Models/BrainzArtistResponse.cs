using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.MusicBrainz.Models
{
    internal sealed class BrainzArtistResponse
    {
        [JsonPropertyName("artists")]
        public List<BrainzArtist> Artists { get; init; } = [];
    }
}
