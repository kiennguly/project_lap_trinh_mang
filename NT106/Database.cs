using System;
using System.Net;                     // 🔹 dùng HttpStatusCode
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;         // 🔹 dùng async/await
using System.Text;
using System.Text.Json;

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

        // nếu backend trả Online thì deserialize luôn cũng được
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
        // URL API Gateway 
        private static readonly string ApiBaseUrl =
            "https://4xt8f352xe.execute-api.ap-southeast-1.amazonaws.com/";
        // Base dành riêng cho MatchHistory
        private static readonly string MatchApiBaseUrl =
            "https://840blg9a68.execute-api.ap-southeast-1.amazonaws.com/";

        private static readonly HttpClient client = new HttpClient();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        // ==============================
        // 1️⃣ ĐĂNG NHẬP
        // ==============================
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
                        // Nếu body không phải JSON, dùng raw luôn
                    }

                    if (response.StatusCode == HttpStatusCode.Conflict) // 409 - đang online nơi khác
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
                    AccountData.Password = password;      // lưu pass người dùng nhập
                    AccountData.Email = account.Email;    // lưu Email

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

        // ==============================
        // 2️⃣ LOAD ACCOUNT THEO USERNAME
        // ==============================
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

        // ==============================
        // 3️⃣ ĐĂNG KÝ (CÓ EMAIL)
        // ==============================
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

        // ==============================
        // 4️⃣ CẬP NHẬT ACCOUNT (VÀNG, LV…)
        // ==============================
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

        // ==============================
        // 5️⃣ QUÊN MẬT KHẨU – GỬI MÃ
        // ==============================
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

        // ==============================
        // 6️⃣ QUÊN MẬT KHẨU – XÁC NHẬN MÃ + ĐỔI MẬT KHẨU
        // ==============================
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

        // ==============================
        // 7️⃣ ĐỔI MẬT KHẨU TRỰC TIẾP (ĐANG ĐĂNG NHẬP)
        // ==============================
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

        // ==============================
        // 8️⃣ LƯU LỊCH SỬ ĐẤU
        // ==============================
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

        // ==============================
        // 9️⃣ LẤY LỊCH SỬ ĐẤU
        // ==============================
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

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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

        // ==============================
        // 🔟 SET ONLINE / OFFLINE
        // ==============================
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
    }
}
