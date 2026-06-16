using MusicAggregator.Application.Abstractions;
using System.Collections.Concurrent;
using static MusicAggregator.Application.Abstractions.IApiStatsStore;

namespace MusicAggregator.Infrastructure.Statistics
{
    internal sealed class ApiStatsStore : IApiStatsStore
    {
        private readonly ConcurrentDictionary<string, ApiStats> _stats = new();

        public void Record(string api, long elapsedMs) => _stats.GetOrAdd(api, _ => new ApiStats()).Record(elapsedMs);

        public IReadOnlyList<ApiStatsSnapshot> GetSnapshot() => _stats.Select(kvp => kvp.Value.ToSnapshot(kvp.Key)).ToArray();
    }
}
