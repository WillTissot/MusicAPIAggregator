using System.Text.Json.Serialization;

namespace MusicAggregator.Infrastructure.Deezer.Models
{
    internal class DeezerTrack
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; } = "";

        [JsonPropertyName("title_version")]
        public string? TitleVersion { get; init; }  

        [JsonPropertyName("isrc")]
        public string? Isrc { get; init; }

        [JsonPropertyName("link")]
        public string? Link { get; init; }

        [JsonPropertyName("duration")]
        public int Duration { get; init; }          

        [JsonPropertyName("rank")]
        public int Rank { get; init; }

        [JsonPropertyName("explicit_lyrics")]
        public bool ExplicitLyrics { get; init; }

        [JsonPropertyName("preview")]
        public string? Preview { get; init; }   

        [JsonPropertyName("artist")]
        public DeezerArtist Artist { get; init; } = new();

        [JsonPropertyName("album")]
        public DeezerAlbum Album { get; init; } = new();
    }
}
