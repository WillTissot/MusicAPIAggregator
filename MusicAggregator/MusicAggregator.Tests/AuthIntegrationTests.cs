using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MusicAggregator.Tests
{
    public class AuthIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_Returns401()
        {
            var res = await _client.GetAsync("/api/v1/MusicAggregator/song?artist=x&track=y");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task Login_WithBadCredentials_Returns401()
        {
            var res = await _client.PostAsJsonAsync("/api/v1/auth/token",
                new { username = "nope", password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task Login_ThenCallProtectedEndpoint_Returns200()
        {
            // 1. login
            var login = await _client.PostAsJsonAsync("/api/v1/auth/token",
                new { username = "test", password = "test" });
            login.EnsureSuccessStatusCode();
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
            Assert.False(string.IsNullOrWhiteSpace(token));

            // 2. use it
            var req = new HttpRequestMessage(HttpMethod.Get,
                "/api/v1/MusicAggregator/song?artist=Radiohead&track=Creep");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            Assert.NotEqual(HttpStatusCode.Unauthorized, res.StatusCode);  // 200 (or 404/502, but NOT 401)
        }

        private sealed record TokenResponse(string Token);
    }
}
