using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    // Model account từ Lambda
    public class AccountModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

        public int Gold { get; set; }
        public int UpgradeHP { get; set; }
        public int UpgradeDamage { get; set; }
        public int Level { get; set; }

        public bool RewardLv10Claimed { get; set; }
        public bool RewardLv50Claimed { get; set; }
        public bool RewardLv100Claimed { get; set; }

        public bool Online { get; set; }
    }

    // Model lịch sử đấu
    public class ClientMatchHistoryModel
    {
        public string? Id { get; set; }
        public string? WinnerUsername { get; set; }
        public string? LoserUsername { get; set; }
        public string? MatchDate { get; set; }
    }

    public static class Database
    {
        // URL API của account
        private static readonly string ApiBaseUrl =
            "https://4xt8f352xe.execute-api.ap-southeast-1.amazonaws.com/";

        // URL API của lịch sử đấu
        private static readonly string MatchApiBaseUrl =
            "https://840blg9a68.execute-api.ap-southeast-1.amazonaws.com/";

        // URL API của Friend
        private static readonly string FriendApiBaseUrl =
            "https://sotv808c91.execute-api.ap-southeast-1.amazonaws.com/";

        private static readonly HttpClient client = new HttpClient();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        // =========================================================
        //                    ACCOUNT / LOGIN
        // =========================================================

        //  ĐĂNG NHẬP
        public static bool CheckLogin(string username, string password)
        {
            try
            {
                var bodyData = new { Username = username, Password = password };
                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(ApiBaseUrl + "account/login", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var raw = response.Content.ReadAsStringAsync().Result;

                    // Thử parse JSON để lấy field "message"
                    string userMsg = raw;
                    try
                    {
                        using var doc = JsonDocument.Parse(raw);
                        if (doc.RootElement.TryGetProperty("message", out var msgProp))
                        {
                            userMsg = msgProp.GetString() ?? raw;
                        }
                    }
                    catch
                    {
                        // Nếu body không phải JSON, dùng raw 
                    }

                    if (response.StatusCode == HttpStatusCode.Conflict) // đang online nơi khác
                    {
                        MessageBox.Show(
                            userMsg,
                            "Tài khoản đang đăng nhập",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            "Đăng nhập thất bại! " + userMsg,
                            "Lỗi Đăng nhập",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }

                    return false;
                }

                var account = response.Content.ReadFromJsonAsync<AccountModel>(JsonOptions).Result;
                if (account != null)
                {
                    AccountData.Username = account.Username;
                    AccountData.Password = password;
                    AccountData.Email = account.Email;

                    AccountData.Gold = account.Gold;
                    AccountData.UpgradeHP = account.UpgradeHP;
                    AccountData.UpgradeDamage = account.UpgradeDamage;
                    AccountData.Level = account.Level;
                    AccountData.RewardLv10Claimed = account.RewardLv10Claimed;
                    AccountData.RewardLv50Claimed = account.RewardLv50Claimed;
                    AccountData.RewardLv100Claimed = account.RewardLv100Claimed;

                    return true;
                }

                MessageBox.Show("Đăng nhập thất bại! Không nhận được dữ liệu account.");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi đăng nhập: " + ex.Message);
                return false;
            }
        }

        // LOAD ACCOUNT THEO USERNAME
        public static bool LoadAccountData(string username)
        {
            try
            {
                var response = client.GetAsync(ApiBaseUrl + "account/" + username).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        $"Tải dữ liệu thất bại! API trả về {response.StatusCode}. Chi tiết: {msg}"
                    );
                    return false;
                }

                var account = response.Content.ReadFromJsonAsync<AccountModel>(JsonOptions).Result;
                if (account != null)
                {
                    AccountData.Username = account.Username;
                    AccountData.Email = account.Email;

                    AccountData.Gold = account.Gold;
                    AccountData.UpgradeHP = account.UpgradeHP;
                    AccountData.UpgradeDamage = account.UpgradeDamage;
                    AccountData.Level = account.Level;

                    AccountData.RewardLv10Claimed = account.RewardLv10Claimed;
                    AccountData.RewardLv50Claimed = account.RewardLv50Claimed;
                    AccountData.RewardLv100Claimed = account.RewardLv100Claimed;

                    return true;
                }

                MessageBox.Show("Tải dữ liệu thất bại! Không nhận được account.");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi tải dữ liệu: " + ex.Message);
                return false;
            }
        }

        //  ĐĂNG KÝ (CÓ EMAIL)
        public static bool RegisterAccount(string username, string password, string email)
        {
            try
            {
                var bodyData = new
                {
                    Username = username,
                    Password = password,
                    Email = email
                };

                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(ApiBaseUrl + "account/register", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        $"Đăng ký thất bại! {msg}",
                        "Lỗi Đăng ký",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
                return false;
            }
        }

        // CẬP NHẬT ACCOUNT (VÀNG, LV…)
        public static void UpdateAccountData()
        {
            try
            {
                if (string.IsNullOrEmpty(AccountData.Username))
                {
                    MessageBox.Show(
                        "Không có Username để cập nhật account!",
                        "Lỗi dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                var bodyData = new
                {
                    Username = AccountData.Username,
                    Gold = AccountData.Gold,
                    UpgradeHP = AccountData.UpgradeHP,
                    UpgradeDamage = AccountData.UpgradeDamage,
                    Level = AccountData.Level,

                    RewardLv10Claimed = AccountData.RewardLv10Claimed,
                    RewardLv50Claimed = AccountData.RewardLv50Claimed,
                    RewardLv100Claimed = AccountData.RewardLv100Claimed
                };

                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PutAsync(ApiBaseUrl + "account/update", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        "Cập nhật thất bại! " + msg,
                        "Lỗi Cập nhật",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        // =========================================================
        //                    QUÊN / ĐỔI MẬT KHẨU
        // =========================================================

        //  QUÊN MẬT KHẨU – GỬI MÃ
        public static bool RequestResetCode(string username, string email)
        {
            try
            {
                var bodyData = new
                {
                    Username = username,
                    Email = email
                };

                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(ApiBaseUrl + "account/request-reset", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        "Yêu cầu quên mật khẩu thất bại: " + msg,
                        "Lỗi quên mật khẩu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối khi gửi yêu cầu quên mật khẩu: " + ex.Message,
                    "Lỗi quên mật khẩu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        //  QUÊN MẬT KHẨU – XÁC NHẬN MÃ + ĐỔI MẬT KHẨU
        public static bool ConfirmResetPassword(string username, string email, string code, string newPassword)
        {
            try
            {
                var bodyData = new
                {
                    Username = username,
                    Email = email,
                    Code = code,
                    NewPassword = newPassword
                };

                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(ApiBaseUrl + "account/confirm-reset", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        "Xác nhận mã/đổi mật khẩu thất bại: " + msg,
                        "Lỗi đổi mật khẩu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối khi xác nhận mã: " + ex.Message,
                    "Lỗi đổi mật khẩu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        //  ĐỔI MẬT KHẨU TRỰC TIẾP (ĐANG ĐĂNG NHẬP)
        public static bool ChangePassword(string username, string newPassword)
        {
            try
            {
                var bodyData = new
                {
                    Username = username,
                    NewPassword = newPassword
                };

                string jsonBody = JsonSerializer.Serialize(bodyData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(ApiBaseUrl + "account/change-password", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        "Đổi mật khẩu thất bại: " + msg,
                        "Lỗi đổi mật khẩu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối khi đổi mật khẩu: " + ex.Message,
                    "Lỗi đổi mật khẩu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        // =========================================================
        //                    MATCH HISTORY
        // =========================================================

        //  LƯU LỊCH SỬ ĐẤU
        public static void RecordMatchHistory(string winnerUsername, string loserUsername)
        {
            try
            {
                var matchData = new
                {
                    WinnerUsername = winnerUsername,
                    LoserUsername = loserUsername
                };

                string jsonBody = JsonSerializer.Serialize(matchData, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(MatchApiBaseUrl + "matchhistory/add", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        $"Ghi lịch sử đấu thất bại: {response.StatusCode}\n{msg}",
                        "MatchHistory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi ghi lịch sử đấu: " + ex.Message, "MatchHistory");
            }
        }

        //  LẤY LỊCH SỬ ĐẤU
        public static List<ClientMatchHistoryModel> GetMatchHistory(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Lỗi: Không có Username để tải lịch sử đấu.",
                    "MatchHistory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<ClientMatchHistoryModel>();
            }

            try
            {
                var response = client.GetAsync(MatchApiBaseUrl + "matchhistory/" + username).Result;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<ClientMatchHistoryModel>();

                if (!response.IsSuccessStatusCode)
                {
                    var msg = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(
                        $"Tải lịch sử đấu thất bại: {response.StatusCode}\n{msg}",
                        "MatchHistory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return new List<ClientMatchHistoryModel>();
                }

                var list = response.Content
                    .ReadFromJsonAsync<List<ClientMatchHistoryModel>>(JsonOptions)
                    .Result;

                return list ?? new List<ClientMatchHistoryModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi tải lịch sử đấu: " + ex.Message, "MatchHistory");
                return new List<ClientMatchHistoryModel>();
            }
        }

        // =========================================================
        //                    ONLINE / OFFLINE
        // =========================================================

        // SET ONLINE / OFFLINE
        public static async Task<bool> SetOnlineStatusAsync(string username, bool online)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            try
            {
                var body = new
                {
                    Username = username,
                    Online = online
                };

                string jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(ApiBaseUrl + "account/set-status", content);

                // Không show MessageBox khi thoát game cho đỡ phiền
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        //                    FRIEND API
        // =========================================================

        public static async Task<List<FriendEntry>> GetFriendListAsync(string username)
        {
            var result = new List<FriendEntry>();

            try
            {
                var url = FriendApiBaseUrl + "friend/list/" + username;
                var resp = await client.GetAsync(url);
                var text = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        $"Tải danh sách bạn bè thất bại.\nStatus: {resp.StatusCode}\nBody: {text}",
                        "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return result;
                }

                using var doc = JsonDocument.Parse(text);

                if (!doc.RootElement.TryGetProperty("friends", out var friendsJson) ||
                    friendsJson.ValueKind != JsonValueKind.Array)
                {
                    return result;
                }

                foreach (var f in friendsJson.EnumerateArray())
                {
                    var entry = new FriendEntry();

                    if (f.TryGetProperty("Username", out var u))
                        entry.Username = u.GetString() ?? "";

                    if (f.TryGetProperty("Status", out var s))
                        entry.Status = s.GetString() ?? "";

                    if (f.TryGetProperty("AvatarKey", out var k))
                        entry.AvatarKey = k.GetString() ?? "";

                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bạn bè: " + ex.Message,
                    "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        public static async Task<bool> SendFriendRequestAsync(string fromUsername, string toUsername)
        {
            try
            {
                var body = new
                {
                    fromUsername,
                    toUsername
                };

                string jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(FriendApiBaseUrl + "friend/send", content);
                var text = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Gửi lời mời kết bạn thất bại:\n" + text,
                        "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi lời mời kết bạn: " + ex.Message,
                    "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> RespondFriendRequestAsync(string fromUsername, string toUsername, bool accept)
        {
            try
            {
                var body = new
                {
                    fromUsername,
                    toUsername,
                    accept
                };

                string jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(FriendApiBaseUrl + "friend/respond", content);
                var text = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Phản hồi lời mời kết bạn thất bại:\n" + text,
                        "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi phản hồi lời mời kết bạn: " + ex.Message,
                    "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        // KIỂM TRA TÀI KHOẢN CÓ TỒN TẠI KHÔNG (dùng khi kết bạn)
        public static async Task<bool> CheckAccountExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            try
            {
                var resp = await client.GetAsync(ApiBaseUrl + "account/" + username);
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    // Username không tồn tại
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    // Lỗi khác (API hỏng, mạng, v.v.)
                    MessageBox.Show(
                        $"Không kiểm tra được tài khoản \"{username}\".\n" +
                        $"Status: {resp.StatusCode}\nBody: {body}",
                        "Friend",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                // 200 OK → tài khoản tồn tại
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối khi kiểm tra tài khoản: " + ex.Message,
                    "Friend",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

    }
}
