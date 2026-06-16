using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.LRCLIB.Models;
using System.Net;
using System.Net.Http.Json;

namespace MusicAggregator.Infrastructure.LRCLIB
{
    internal sealed class LrclibClient : ILyricsProvider
    {
        private readonly HttpClient _client;
        private readonly HybridCache _cache;
        private readonly ILogger<LrclibClient> _logger;

        public LrclibClient(HttpClient client, HybridCache cache, ILogger<LrclibClient> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public async Task<LyricsInfo?> GetLyricsInfoAsync(string artist, string track, CancellationToken ct)
        {
            var key = $"lrclip:artist:{Normalize(artist)}:track:{Normalize(track)}";

            return await _cache.GetOrCreateAsync(
                key,
                async token => await FetchLyricsInfoAsync(artist, track, token),
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(24) }, //lyrics don't change often, so we can cache them for a long time
                cancellationToken: ct);
        }

        private async Task<LyricsInfo?> FetchLyricsInfoAsync(string artist, string track, CancellationToken ct)
        {
            string url = $"api/get?artist_name={Uri.EscapeDataString(artist)}" + $"&track_name={Uri.EscapeDataString(track)}";

            using HttpResponseMessage? response = await _client.GetAsync(url, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("LRCLIB has no lyrics for {Artist} - {Track}", artist, track);
                return null;
            }

            response.EnsureSuccessStatusCode();

            LrclibResponse? lyricResponse = await response.Content.ReadFromJsonAsync<LrclibResponse>(ct);

            return lyricResponse?.ToLyricsInfo();
        }

        private static string Normalize(string s) => s.Trim().ToLowerInvariant();
    }
}
