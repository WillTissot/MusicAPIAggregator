using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Abstractions
{
    public interface ITrackProvider
    {
        Task<TrackInfo?> SearchTrackAsync(string artist, string track, CancellationToken ct);
    }
}
