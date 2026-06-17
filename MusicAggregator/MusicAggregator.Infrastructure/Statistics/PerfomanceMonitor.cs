using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;

namespace MusicAggregator.Infrastructure.Statistics
{
    public sealed class PerformanceMonitor(IApiStatsStore stats, ILogger<PerformanceMonitor> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
        private const double SlowRatioThreshold = 0.5;  
        private const long MinRequests = 10;           

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    Analyze();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Performance monitor cycle failed.");
                }
            }
        }

        private void Analyze()
        {
            foreach (var s in stats.GetSnapshot())
            {
                if (s.TotalRequests < MinRequests)
                    continue;

                var slowRatio = (double)s.Buckets.Slow / s.TotalRequests;

                if (slowRatio >= SlowRatioThreshold)
                {
                    logger.LogWarning(
                        "Performance anomaly: {Api} — {Slow}/{Total} requests ({Pct:P0}) were slow (avg {Avg:F0} ms).",
                        s.Api, s.Buckets.Slow, s.TotalRequests, slowRatio, s.AverageResponseMs);
                }
            }
        }
    }
}
