
namespace MusicAggregator.Application.Abstractions
{
    public interface IApiStatsStore
    {
        void Record(string api, long elapsedMs);

        IReadOnlyList<ApiStatsSnapshot> GetSnapshot();

        public sealed record ApiStatsSnapshot(string Api, long TotalRequests, double AverageResponseMs, BucketCounts Buckets);

        public sealed record BucketCounts(long Fast, long Average, long Slow);
    }
}
