using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicAggregator.Application.Abstractions.IApiStatsStore;

namespace MusicAggregator.Infrastructure.Statistics
{
    internal sealed class ApiStats
    {
        private long _count;
        private long _totalMs;
        private long _fast, _average, _slow;

        public void Record(long elapsedMs)
        {
            Interlocked.Increment(ref _count);
            Interlocked.Add(ref _totalMs, elapsedMs);

            if (elapsedMs < 100) Interlocked.Increment(ref _fast);
            else if (elapsedMs <= 150) Interlocked.Increment(ref _average);
            else Interlocked.Increment(ref _slow);
        }

        public ApiStatsSnapshot ToSnapshot(string api)
        {
            var count = Interlocked.Read(ref _count);
            var totalMs = Interlocked.Read(ref _totalMs);
            var avg = count == 0 ? 0 : (double)totalMs / count;

            return new ApiStatsSnapshot(
                api,
                count,
                Math.Round(avg, 2),
                new BucketCounts(
                    Interlocked.Read(ref _fast),
                    Interlocked.Read(ref _average),
                    Interlocked.Read(ref _slow)));
        }
    }
}
