using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.Deezer.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace MusicAggregator.Infrastructure.Deezer
{
    internal class DeezerClient : ITrackProvider
    {
        private readonly HttpClient _client;
        private readonly ILogger<DeezerClient> _logger;

        public DeezerClient(HttpClient client, ILogger<DeezerClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<TrackInfo?> GetTrackAsync(string artist, string track, CancellationToken ct)
        {
            string query = $"artist:\"{artist}\" track:\"{track}\"";
            string url = $"search?q={Uri.EscapeDataString(query)}";

            DeezerSearchResponse? response = await _client.GetFromJsonAsync<DeezerSearchResponse>(url, ct);

            DeezerTrack? bestMatchedTrack = response?.Data.OrderByDescending(d => d.Rank).FirstOrDefault();

            if (bestMatchedTrack is null)
            {
                _logger.LogWarning("Deezer returned no track for {Artist} - {Track}", artist, track);

                return null;
            }

            return bestMatchedTrack.ToTrackInfo();
        }

        public async Task<IReadOnlyList<TrackInfo>?> SearchTracksAsync(string query, CancellationToken ct)
        {
            var url = $"search?q={Uri.EscapeDataString(query)}&limit=25";

            var response = await _client.GetFromJsonAsync<DeezerSearchResponse>(url, ct);

            var tracks = response?.Data ?? [];
            _logger.LogDebug("Deezer search '{Query}' returned {Count} tracks", query, tracks.Count);

            return tracks.Select(t => t.ToTrackInfo()).ToArray();
        }
    }
}
