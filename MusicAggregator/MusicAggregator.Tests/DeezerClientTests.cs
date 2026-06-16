using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MusicAggregator.Infrastructure.Deezer;
using MusicAggregator.Tests.Fakes;
using System.Net;

namespace MusicAggregator.Tests
{
    public class DeezerClientTests
    {
        private static DeezerClient CreateSut(StubHttpMessageHandler handler)
        {
            var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.deezer.com/") };
            return new DeezerClient(http, NullLogger<DeezerClient>.Instance);
        }

        // 1 — maps fields correctly from real JSON
        [Fact]
        public async Task SearchTrack_maps_fields_from_response()
        {
            var json = await File.ReadAllTextAsync("TestData/deezer-search-creep.json");
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetTrackAsync("Radiohead", "Creep", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Title.Should().Be("Creep");
            result.DurationSeconds.Should().Be(238);
            result.AlbumTitle.Should().Be("Pablo Honey");
            result.CoverUrl.Should().StartWith("https://");
            result.ArtistName.Should().Be("Radiohead");
            result.ExplicitLyrics.Should().BeTrue();
        }

        // 2 — picks the highest-rank track when several match
        [Fact]
        public async Task SearchTrack_picks_highest_rank()
        {
            const string json = """
            { "data": [
                { "title": "Creep (Live)", "rank": 100, "duration": 250,
                "explicit_lyrics": false, "artist": { "name": "Radiohead" }, "album": { "title": "Live" } },
                { "title": "Creep",        "rank": 999, "duration": 238,
                "explicit_lyrics": true,  "artist": { "name": "Radiohead" }, "album": { "title": "Pablo Honey" } }
            ], "total": 2 }
            """;
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetTrackAsync("Radiohead", "Creep", CancellationToken.None);

            result!.AlbumTitle.Should().Be("Pablo Honey");   // the rank 999 one
        }

        // 3 — no results → null (graceful, not an exception)
        [Fact]
        public async Task SearchTrack_returns_null_when_no_results()
        {
            var sut = CreateSut(new StubHttpMessageHandler("""{ "data": [], "total": 0 }"""));

            var result = await sut.GetTrackAsync("zzz", "zzz", CancellationToken.None);

            result.Should().BeNull();
        }

        // 4 — builds the correct, encoded request URL
        [Fact]
        public async Task SearchTrack_builds_correct_query()
        {
            var handler = new StubHttpMessageHandler("""{ "data": [], "total": 0 }""");
            var sut = CreateSut(handler);

            await sut.GetTrackAsync("Radiohead", "Creep", CancellationToken.None);

            handler.LastRequest!.RequestUri!.ToString()
                .Should().Contain("search?q=")
                .And.Contain("artist")
                .And.Contain("Radiohead");   // encoded form is fine to assert loosely
        }

        // 5 — upstream failure propagates
        [Fact]
        public async Task SearchTrack_throws_on_server_error()
        {
            var sut = CreateSut(new StubHttpMessageHandler("error", HttpStatusCode.InternalServerError));

            var act = () => sut.GetTrackAsync("Radiohead", "Creep", CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
