using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    public static class RoomApi
    {
        // POST /sololan  (API Gateway HTTP API -> Lambda sololan)
        private const string ROOM_API_URL =
            "https://r42i9q5tl1.execute-api.ap-southeast-1.amazonaws.com/sololan";
        // ↑ sửa lại đúng Invoke URL / route của bạn nếu khác

        private static readonly HttpClient client = new HttpClient();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // ================== HÀM DÙNG CHUNG (POST JSON) ==================

        private static async Task<bool> PostAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.PostAsync(ROOM_API_URL, content);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================== CÁC ACTION PHÒNG ==================

        // Host tạo phòng
        public static Task<bool> CreateRoomAsync(string roomId, string hostName)
        {
            return PostAsync(new
            {
                action = "create",
                roomId = roomId,
                host = hostName
            });
        }

        // Client join phòng
        public static Task<bool> JoinRoomAsync(string roomId, string guestName)
        {
            return PostAsync(new
            {
                action = "join",
                roomId = roomId,
                guest = guestName
            });
        }

        // Host bấm BẮT ĐẦU
        public static Task<bool> StartRoomAsync(string roomId, string hostName)
        {
            return PostAsync(new
            {
                action = "start",
                roomId = roomId,
                host = hostName
            });
        }

        // ĐÁNH DẤU PHÒNG KẾT THÚC (END)
        public static Task<bool> EndRoomAsync(string roomId)
        {
            return PostAsync(new
            {
                action = "end",
                roomId = roomId
            });
        }

        // Khi client B out, host đưa phòng về CREATED (1/2)
        public static Task<bool> BackToCreatedAsync(string roomId)
        {
            return PostAsync(new
            {
                action = "back_to_created",
                roomId = roomId
            });
        }

        // Hủy phòng khi host thoát TRƯỚC khi bấm BẮT ĐẦU
        public static Task<bool> CancelRoomAsync(string roomId)
        {
            return PostAsync(new
            {
                action = "cancel",
                roomId = roomId
            });
        }

        // ================== LẤY DANH SÁCH PHÒNG (action = list) ==================

        public class RoomInfo
        {
            public string RoomId { get; set; } = "";
            public string Host { get; set; } = "";
            public int PlayerCount { get; set; }         // 1 hoặc 2
            public string Status { get; set; } = "";     // CREATED, READY, STARTED...
        }

        private class RoomListResponse
        {
            public bool Ok { get; set; }
            public List<RoomInfo>? Rooms { get; set; }
        }

        public static async Task<List<RoomInfo>> GetRoomsAsync()
        {
            try
            {
                var payload = new { action = "list" };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(ROOM_API_URL, content);
                if (!resp.IsSuccessStatusCode)
                    return new List<RoomInfo>();

                var respString = await resp.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<RoomListResponse>(respString, JsonOptions);
                return result?.Rooms ?? new List<RoomInfo>();
            }
            catch
            {
                return new List<RoomInfo>();
            }
        }
    }
}
