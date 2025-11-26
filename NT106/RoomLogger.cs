using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    public static class RoomLogger
    {
        private const string API_BASE = "https://r4zi9q5t1l.execute-api.ap-southeast-1.amazonaws.com";

        private static readonly HttpClient http = new HttpClient();
        private static readonly JsonSerializerOptions jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Gửi log phòng lên Lambda / DynamoDB.
        /// host hoặc guest có thể null.
        /// </summary>
        private static async Task SendAsync(string roomId, string host, string guest)
        {
            try
            {
                var payload = new
                {
                    roomId = roomId,
                    host = host,   // có thể null
                    guest = guest  // có thể null
                };

                string json = JsonSerializer.Serialize(payload, jsonOpts);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                // POST /sololan hoặc /rooms tùy cấu hình route
                await http.PostAsync($"{API_BASE}/sololan", content);
                // Nếu route của là /rooms thì đổi lại:
                // await http.PostAsync($"{API_BASE}/rooms", content);
            }
            catch
            {
                // Không cho game crash vì lỗi mạng/AWS
            }
        }

        /// <summary>
        /// Host tạo phòng.
        /// </summary>
        public static Task LogHost(string roomId, string hostName)
            => SendAsync(roomId, hostName, null);

        /// <summary>
        /// Guest join phòng.
        /// </summary>
        public static Task LogGuest(string roomId, string guestName)
            => SendAsync(roomId, null, guestName);
    }
}
