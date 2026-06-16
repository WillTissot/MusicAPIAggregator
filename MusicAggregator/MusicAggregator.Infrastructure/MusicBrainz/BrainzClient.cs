using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.MusicBrainz.Models;
using System.Net.Http.Json;

namespace MusicAggregator.Infrastructure.MusicBrainz
{
    internal sealed class BrainzClient : IArtistProvider
    {
        private readonly HttpClient _client;
        private readonly HybridCache _cache;
        private readonly ILogger<BrainzClient> _logger;

        public BrainzClient(HttpClient client, HybridCache cache, ILogger<BrainzClient> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }   

        public async Task<ArtistInfo?> GetArtistAsync(string artist, CancellationToken ct)
        {
            var key = $"brainz:artist:{Normalize(artist)}";

            return await _cache.GetOrCreateAsync(
                key,
                async token => await FetchArtistAsync(artist, token),
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(24) }, //artist info doesn't change often, so we can cache it for a day
                cancellationToken: ct);
        }

        public async Task<IReadOnlyList<ArtistInfo>?> SearchArtistsAsync(string query, CancellationToken ct)
        {
            var key = $"brainz:search:{Normalize(query)}";

            return await _cache.GetOrCreateAsync(
                key,
                async token => await FetchArtistsAsync(query, token),
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(24) }, //artist info doesn't change often, so we can cache it for a day
                cancellationToken: ct);
        }

        private async Task<ArtistInfo?> FetchArtistAsync(string artist, CancellationToken ct)
        {
            var url = $"artist?query={Uri.EscapeDataString(artist)}&fmt=json";

            var response = await _client.GetFromJsonAsync<BrainzArtistResponse>(url, ct);
            var best = response?.Artists.OrderByDescending(a => a.Score).FirstOrDefault();

            if (best is null)
            {
                _logger.LogInformation("MusicBrainz returned no artist for {Artist}", artist);
                return null;
            }
            return best.ToArtistInfo();
        }

        private async Task<IReadOnlyList<ArtistInfo>> FetchArtistsAsync(string query, CancellationToken ct)
        {
            var url = $"artist?query={Uri.EscapeDataString(query)}&fmt=json";

            var response = await _client.GetFromJsonAsync<BrainzArtistResponse>(url, ct);
            var artists = response?.Artists ?? [];
            _logger.LogDebug("MusicBrainz search '{Query}' returned {Count} artists", query, artists.Count);
            return artists.Select(a => a.ToArtistInfo()).ToArray();
        }

        private static string Normalize(string s) => s.Trim().ToLowerInvariant();
    }
}
