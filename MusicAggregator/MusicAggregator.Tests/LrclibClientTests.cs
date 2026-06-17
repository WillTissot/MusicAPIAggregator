using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MusicAggregator.Infrastructure.LRCLIB;
using MusicAggregator.Tests.Fakes;
using System.Net;

namespace MusicAggregator.Tests
{
    public class LrclibClientTests
    {
        private static LrclibClient CreateSut(StubHttpMessageHandler handler)
        {
            var http = new HttpClient(handler) { BaseAddress = new Uri("https://lrclib.net/") };
            return new LrclibClient(http, TestCache.Create(), NullLogger<LrclibClient>.Instance);
        }

        // 1 — maps lyrics from the real response
        [Fact]
        public async Task GetLyrics_maps_fields_from_response()
        {
            var json = await File.ReadAllTextAsync("TestData/lrclib-get-creep.json");
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetLyricsInfoAsync("Radiohead", "Creep", CancellationToken.None);

            result.Should().NotBeNull();
            result!.PlainLyrics.Should().StartWith("When you were here before");
            result.SyncedLyrics.Should().Contain("[00:19.16]");   
            result.IsInstrumental.Should().BeFalse();
        }

        // 2 — 404 → null (the graceful-degradation case, NOT an exception)
        [Fact]
        public async Task GetLyrics_returns_null_when_not_found()
        {
            var sut = CreateSut(new StubHttpMessageHandler("Not Found", HttpStatusCode.NotFound));

            var result = await sut.GetLyricsInfoAsync("Nobody", "No Such Song", CancellationToken.None);

            result.Should().BeNull();  
        }

        // 3 — instrumental track: flagged, no lyrics text
        [Fact]
        public async Task GetLyrics_handles_instrumental_track()
        {
            const string json = """
          {
              "instrumental": true,
              "plainLyrics": null,
              "syncedLyrics": null
          }
          """;
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetLyricsInfoAsync("Some Artist", "Interlude", CancellationToken.None);

            result.Should().NotBeNull();
            result!.IsInstrumental.Should().BeTrue();
            result.PlainLyrics.Should().BeNull();
        }

        // 4 — genuine upstream failure (5xx) → throws (orchestrator decides)
        [Fact]
        public async Task GetLyrics_throws_on_server_error()
        {
            var sut = CreateSut(new StubHttpMessageHandler("error", HttpStatusCode.InternalServerError));

            var act = () => sut.GetLyricsInfoAsync("Radiohead", "Creep", CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }

        // 5 — builds the correct, encoded query
        [Fact]
        public async Task GetLyrics_builds_correct_query()
        {
            var handler = new StubHttpMessageHandler("Not Found", HttpStatusCode.NotFound);
            var sut = CreateSut(handler);

            await sut.GetLyricsInfoAsync("Sigur Rós", "Hoppípolla", CancellationToken.None);

            var url = handler.LastRequest!.RequestUri!.ToString();
            url.Should().Contain("api/get?");
            url.Should().Contain("artist_name=");
            url.Should().Contain("track_name=");
            url.Should().Contain("Sigur");          
        }
    }
}
