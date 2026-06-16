using MusicAggregator.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAggregator.Infrastructure.Statistics
{
    internal sealed class ApiStatsHandler : DelegatingHandler
    {
        private readonly IApiStatsStore _store;
        public ApiStatsHandler(IApiStatsStore store) => _store = store;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            var api = ResolveApi(request.RequestUri?.Host);
            var sw = Stopwatch.StartNew();
            try
            {
                return await base.SendAsync(request, ct);
            }
            finally
            {
                sw.Stop();
                _store.Record(api, sw.ElapsedMilliseconds);
            }
        }

        private static string ResolveApi(string? host) => host switch
        {
            "api.deezer.com" => "Deezer",
            "musicbrainz.org" => "MusicBrainz",
            "lrclib.net" => "LRCLIB",
            _ => host ?? "Unknown",
        };
    }
}
