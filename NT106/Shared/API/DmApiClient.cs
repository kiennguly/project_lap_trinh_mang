using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    public static class DmApiClient
    {
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://qlrzbi3707.execute-api.ap-southeast-1.amazonaws.com/")
        };

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public class DmHistoryResponse
        {
            public string? Key { get; set; }
            public string[]? Lines { get; set; }
        }

        // GET /dm?user1=...&user2=...
        public static async Task<DmHistoryResponse?> GetHistoryAsync(string user1, string user2)
        {
            var url = $"dm?user1={Uri.EscapeDataString(user1)}&user2={Uri.EscapeDataString(user2)}";

            using var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                // Debug tạm: xem status nếu cần
                // MessageBox.Show($"GET dm thất bại: {(int)resp.StatusCode}");
                return null;
            }

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DmHistoryResponse>(json, _jsonOptions);
        }

        // POST /dm  { from, to, message }
        public static async Task<bool> AppendMessageAsync(string from, string to, string message)
        {
            var body = new { from, to, message };
            string json = JsonSerializer.Serialize(body);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync("dm", content);
            return resp.IsSuccessStatusCode;
        }
    }
}
