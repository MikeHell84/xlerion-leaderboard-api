using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class ApiIntegrationTests
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;

        public ApiIntegrationTests()
        {
            // Allow overriding the API base URL via environment for CI
            _baseUrl = Environment.GetEnvironmentVariable("TEST_API_BASE") ?? "http://localhost:5000";
            _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            _client.Timeout = TimeSpan.FromSeconds(10);
        }

        [Fact(DisplayName = "Swagger JSON is available")]
        public async Task Swagger_ReturnsJson()
        {
            var resp = await _client.GetAsync("/swagger/v1/swagger.json");
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
            Assert.Contains("openapi", content, StringComparison.OrdinalIgnoreCase | StringComparison.CurrentCulture);
        }

        [Fact(DisplayName = "Leaderboard endpoint returns entries")]
        public async Task Leaderboard_ReturnsEntries()
        {
            var resp = await _client.GetAsync("/api/leaderboard/xlerion-arena?top=5");
            resp.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            // Expect either single object or array; at minimum should contain playerId or rank
            var root = doc.RootElement;
            bool ok = false;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                ok = root[0].TryGetProperty("playerId", out _) || root[0].TryGetProperty("rank", out _);
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                ok = root.TryGetProperty("playerId", out _) || root.TryGetProperty("rank", out _);
            }
            Assert.True(ok, "Leaderboard response did not contain expected fields");
        }

        [Fact(DisplayName = "Submit score creates new score record")]
        public async Task SubmitScore_CreatesScore()
        {
            var payload = new
            {
                playerId = 1,
                gameId = "xlerion-arena",
                value = 123
            };

            var resp = await _client.PostAsJsonAsync("/api/scores", payload);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("id", out _), "Response JSON did not contain 'id' field");
        }
    }
}
