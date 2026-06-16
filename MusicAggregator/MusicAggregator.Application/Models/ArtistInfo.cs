
namespace MusicAggregator.Application.Models
{
    public sealed class ArtistInfo
    {
        public required string Name { get; init; }
        public string? Country { get; init; } 
        public string? Type { get; init; }          
        public string? FormedYear { get; init; }     
        public IReadOnlyList<string> Tags { get; init; } = [];   
    }
}
