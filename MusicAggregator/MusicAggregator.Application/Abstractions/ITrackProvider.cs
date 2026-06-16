using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Abstractions
{
    public interface ITrackProvider
    {
        Task<TrackInfo?> GetTrackAsync(string artist, string track, CancellationToken ct);
        Task<IReadOnlyList<TrackInfo>?> SearchTracksAsync(string query, CancellationToken ct);
    }
}
