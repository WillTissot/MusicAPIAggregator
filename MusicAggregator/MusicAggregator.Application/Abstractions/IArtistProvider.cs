using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Abstractions
{
    public interface IArtistProvider
    {
        Task<ArtistInfo?> GetArtistAsync(string artist, CancellationToken ct);
    }
}
