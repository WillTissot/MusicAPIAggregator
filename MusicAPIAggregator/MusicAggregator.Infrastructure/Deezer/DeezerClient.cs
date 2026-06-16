using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;

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

        public Task<TrackInfo?> SearchTrackAsync(string artist, string track, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
