using Microsoft.Extensions.Logging;
using MusicAggregator.Application.Abstractions;
using MusicAggregator.Application.Models;
using MusicAggregator.Infrastructure.LRCLIB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MusicAggregator.Infrastructure.LRCLIB
{
    internal sealed class LrclibClient : ILyricsProvider
    {
        private readonly HttpClient _client;
        private readonly ILogger<LrclibClient> _logger;

        public LrclibClient(HttpClient client, ILogger<LrclibClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<LyricsInfo?> GetLyricsInfoAsync(string artist, string track, CancellationToken ct)
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
    }
}
