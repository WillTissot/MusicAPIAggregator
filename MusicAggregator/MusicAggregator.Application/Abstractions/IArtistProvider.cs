using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Abstractions
{
    public interface IArtistProvider
    {
        Task<ArtistInfo?> GetArtistAsync(string artist, CancellationToken ct);
        Task<IReadOnlyList<ArtistInfo>?> SearchArtistsAsync(string query, CancellationToken ct);
    }
}
