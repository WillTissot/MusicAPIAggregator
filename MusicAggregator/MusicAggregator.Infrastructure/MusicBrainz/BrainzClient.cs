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
        private readonly ILogger<BrainzClient> _logger;

        public BrainzClient(HttpClient client, ILogger<BrainzClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ArtistInfo?> GetArtistAsync(string artist, CancellationToken ct)
        {
            string url = $"artist?query={Uri.EscapeDataString(artist)}&fmt=json";

            BrainzArtistResponse? response = await _client.GetFromJsonAsync<BrainzArtistResponse>(url, ct);

            BrainzArtist? best = response?.Artists.OrderByDescending(a => a.Score).FirstOrDefault(); 

            if (best is null)
            {
                _logger.LogInformation("MusicBrainz returned no artist for {Artist}", artist);
                return null;
            }

            return best.ToArtistInfo();
        }
    }
}
