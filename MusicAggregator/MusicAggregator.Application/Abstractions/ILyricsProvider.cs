using MusicAggregator.Application.Models;

namespace MusicAggregator.Application.Abstractions
{
    public interface ILyricsProvider
    {
        Task<LyricsInfo?> GetLyricsInfoAsync(string artist, string track, CancellationToken ct);
    }
}
