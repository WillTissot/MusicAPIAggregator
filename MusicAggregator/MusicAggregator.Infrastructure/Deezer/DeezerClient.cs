using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.Deezer.Models;
using System.Net.Http.Json;

namespace MusicAggregator.Infrastructure.Deezer
{
    internal class DeezerClient : ITrackProvider
    {
        private readonly HttpClient _client;
        private readonly HybridCache _cache;
        private readonly ILogger<DeezerClient> _logger;

        public DeezerClient(HttpClient client, HybridCache cache, ILogger<DeezerClient> logger)
        {
            _client = client;
            _logger = logger;
            _cache = cache;
        }

        public async Task<TrackInfo?> GetTrackAsync(string artist, string track, CancellationToken ct)
        {
            var key = $"deezer:track:{Normalize(artist)}:{Normalize(track)}";

            return await _cache.GetOrCreateAsync(
                key,
                async token => await FetchTrackAsync(artist, track, token),
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(30) },
                cancellationToken: ct);
        }

        public async Task<IReadOnlyList<TrackInfo>?> SearchTracksAsync(string query, CancellationToken ct)
        {
            var key = $"deezer:search:{Normalize(query)}";

            return await _cache.GetOrCreateAsync(
                key,
                async token => await FetchTracksAsync(query, token),
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) }, 
                //Songs change more often. For example artist may release a new single, so we want to refresh the search results more frequently.
                cancellationToken: ct);
        }


        private async Task<TrackInfo?> FetchTrackAsync(string artist, string track, CancellationToken ct)
        {
            var query = $"artist:\"{artist}\" track:\"{track}\"";
            var url = $"search?q={Uri.EscapeDataString(query)}";

            var response = await _client.GetFromJsonAsync<DeezerSearchResponse>(url, ct);
            var best = response?.Data.OrderByDescending(d => d.Rank).FirstOrDefault();

            if (best is null)
            {
                _logger.LogWarning("Deezer returned no track for {Artist} - {Track}", artist, track);
                return null;
            }
            return best.ToTrackInfo();
        }

        private async Task<IReadOnlyList<TrackInfo>> FetchTracksAsync(string query, CancellationToken ct)
        {
            var url = $"search?q={Uri.EscapeDataString(query)}&limit=25";
            var response = await _client.GetFromJsonAsync<DeezerSearchResponse>(url, ct);
            var tracks = response?.Data ?? [];
            _logger.LogDebug("Deezer search '{Query}' returned {Count} tracks", query, tracks.Count);
            return tracks.Select(t => t.ToTrackInfo()).ToArray();
        }

        private static string Normalize(string s) => s.Trim().ToLowerInvariant();
    }
}
