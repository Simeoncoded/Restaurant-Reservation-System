using Microsoft.Extensions.Options;
using RestaurantReservationSystem.Services.OpenAI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RestaurantReservationSystem.Repositories
{
    public class OpenAiChatProvider : IChatProvider
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAiOptions _options;

        public OpenAiChatProvider(HttpClient httpClient, IOptions<OpenAiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, CancellationToken ct = default)
        {
            var body = new
            {
                model = _options.Model,
                temperature = 0.2,
                messages = messages.Select(m => new
                {
                    role = m.role?.Trim().ToLowerInvariant(), // always lowercase
                    content = m.content
                })
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _httpClient.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "";
        }
    }
}
