using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MusicAggregator.Infrastructure.MusicBrainz;
using MusicAggregator.Tests.Fakes;
using System.Net;

namespace MusicAggregator.Tests
{
    public class BrainzClientTests
    {
        private static BrainzClient CreateSut(StubHttpMessageHandler handler)
        {
            var http = new HttpClient(handler) { BaseAddress = new Uri("https://musicbrainz.org/ws/2/") };
            return new BrainzClient(http, NullLogger<BrainzClient>.Instance);
        }

        // 1 — maps the core fields from the real response
        [Fact]
        public async Task GetArtist_maps_fields_from_response()
        {
            var json = await File.ReadAllTextAsync("TestData/brainz-search-radiohead.json");
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetArtistAsync("Radiohead", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Radiohead");
            result.Country.Should().Be("GB");
            result.Type.Should().Be("Group");
            result.FormedYear.Should().Be("1991");
        }

        // 2 — picks the HIGHEST score, not a fuzzy match like "On a Friday" (score 64)
        [Fact]
        public async Task GetArtist_picks_highest_score()
        {
            var json = await File.ReadAllTextAsync("TestData/brainz-search-radiohead.json");
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetArtistAsync("Radiohead", CancellationToken.None);

            result!.Name.Should().Be("Radiohead");// score 100
            result.Name.Should().NotBe("On a Friday");    
        }

        // 3 — tags are filtered (count > 0) and ranked, capped at 5
        [Fact]
        public async Task GetArtist_curates_tags()
        {
            const string json = """
          { "artists": [ {
              "id": "x", "score": 100, "name": "Test", "type": "Group",
              "life-span": { "begin": "2000" },
              "tags": [
                  { "count": 42, "name": "alternative rock" },
                  { "count": 0,  "name": "noise" },
                  { "count": 17, "name": "rock" },
                  { "count": -1, "name": "england" },
                  { "count": 29, "name": "art rock" },
                  { "count": 5,  "name": "british" },
                  { "count": 3,  "name": "indie" },
                  { "count": 1,  "name": "experimental" }
              ]
          } ] }
          """;
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetArtistAsync("Test", CancellationToken.None);

            result!.Tags.Should().HaveCount(5);                          // capped
            result.Tags.Should().NotContain(["noise", "england"]);      // count <= 0 dropped
            result.Tags.Should().StartWith("alternative rock");          // highest count first
            result.Tags.Should().ContainInOrder("alternative rock", "art rock", "rock"); // ranked desc
        }

        // 4 — missing country/type/tags handled
        [Fact]
        public async Task GetArtist_handles_missing_optional_fields()
        {
            const string json = """
          { "artists": [ { "id": "x", "score": 90, "name": "Minimal" } ] }
          """;
            var sut = CreateSut(new StubHttpMessageHandler(json));

            var result = await sut.GetArtistAsync("Minimal", CancellationToken.None);

            result!.Name.Should().Be("Minimal");
            result.Country.Should().BeNull();
            result.Type.Should().BeNull();
            result.FormedYear.Should().BeNull();
            result.Tags.Should().BeEmpty();          // not null — empty
        }

        // 5 — no matches → null (graceful, not an exception)
        [Fact]
        public async Task GetArtist_returns_null_when_no_results()
        {
            var sut = CreateSut(new StubHttpMessageHandler("""{ "artists": [] }"""));

            var result = await sut.GetArtistAsync("zzzz", CancellationToken.None);

            result.Should().BeNull();
        }

        // 6 — builds the correct, encoded query
        [Fact]
        public async Task GetArtist_builds_correct_query()
        {
            var handler = new StubHttpMessageHandler("""{ "artists": [] }""");
            var sut = CreateSut(handler);

            await sut.GetArtistAsync("Sigur Rós", CancellationToken.None);

            var url = handler.LastRequest!.RequestUri!.ToString();
            url.Should().Contain("artist?query=");
            url.Should().Contain("fmt=json");
            url.Should().Contain("Sigur");         
        }

        // 7 — upstream failure propagates
        [Fact]
        public async Task GetArtist_throws_on_server_error()
        {
            var sut = CreateSut(new StubHttpMessageHandler("error", HttpStatusCode.ServiceUnavailable));

            var act = () => sut.GetArtistAsync("Radiohead", CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
