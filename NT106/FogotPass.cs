using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class FogotPass : Form
    {
        //API của backend 
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://4xt8f352xe.execute-api.ap-southeast-1.amazonaws.com/")
        };

        // ----- Cấu hình mail server  -----
        private const string SmtpHost = "smtp.gmail.com";   // ví dụ dùng Gmail
        private const int SmtpPort = 587;
        private const string FromEmail = "minhnhat2k6hcm@gmail.com";       // email gửi
        private const string FromPassword = "hyoo itgr rkyg osmd"; 

        // ----- Biến lưu OTP hiện tại -----
        private string? _currentOtp;
        private DateTime? _otpExpireTime;

        public FogotPass()
        {
            InitializeComponent();
            InitState();
        }

        // Khóa phần nhập OTP + mật khẩu mới lúc đầu
        private void InitState()
        {
            textBoxCode.Enabled = false;
            textBoxNewPass.Enabled = false;
            textBoxConfirmPass.Enabled = false;
            buttonConfirm.Enabled = false;
        }

        // Bật phần Step 2 sau khi gửi mã thành công
        private void EnableStep2()
        {
            textBoxCode.Enabled = true;
            textBoxNewPass.Enabled = true;
            textBoxConfirmPass.Enabled = true;
            buttonConfirm.Enabled = true;

            // Khoá lại Username / Email cho chắc
            textBoxUser.Enabled = false;
            textBoxEmail.Enabled = false;
        }

        // GỬI MÃ OTP VỀ GMAIL
        private async void buttonSendCode_Click(object sender, EventArgs e)
        {
            string username = textBoxUser.Text.Trim();
            string email = textBoxEmail.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Gmail.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Gmail không hợp lệ.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonSendCode.Enabled = false;

            try
            {
                // --- Gọi API GET /account/{username} để kiểm tra tài khoản & email ---
                var resp = await httpClient.GetAsync($"account/{Uri.EscapeDataString(username)}");
                string respBody = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    string msg = ExtractMessage(respBody);
                    if (string.IsNullOrWhiteSpace(msg))
                        msg = "Không tìm thấy tài khoản hoặc lỗi hệ thống.";
                    MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    buttonSendCode.Enabled = true;
                    return;
                }

                // Lấy Email trong DB
                string? emailFromDb = ExtractEmail(respBody);
                if (string.IsNullOrEmpty(emailFromDb))
                {
                    MessageBox.Show("Không đọc được email từ server.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    buttonSendCode.Enabled = true;
                    return;
                }

                if (!string.Equals(emailFromDb.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Gmail không khớp với tài khoản đã đăng ký.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    buttonSendCode.Enabled = true;
                    return;
                }

                // --- Tạo OTP 6 số và lưu lại ở client ---
                var rnd = new Random();
                _currentOtp = rnd.Next(100000, 999999).ToString();
                _otpExpireTime = DateTime.UtcNow.AddMinutes(10);

                // --- Gửi email OTP ---
                await SendOtpEmailAsync(email, username, _currentOtp);

                MessageBox.Show("Đã gửi mã OTP tới Gmail, vui lòng kiểm tra hộp thư.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                EnableStep2();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới máy chủ hoặc gửi mail.\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonSendCode.Enabled = true;
            }
        }

        // Hàm gửi email OTP qua SMTP
        private async Task SendOtpEmailAsync(string toEmail, string username, string otp)
        {
            string subject = "Mã đặt lại mật khẩu - Plane Fighting Super Start";
            string body =
                $"Xin chào {username},\n\n" +
                $"Mã đặt lại mật khẩu của bạn là: {otp}\n" +
                $"Mã này có hiệu lực trong 10 phút.\n\n" +
                $"Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.";

            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(FromEmail, FromPassword);

                using (var mail = new MailMessage(FromEmail, toEmail, subject, body))
                {
                    await client.SendMailAsync(mail);
                }
            }
        }

        // XÁC NHẬN OTP & ĐỔI MẬT KHẨU (update DynamoDB qua API)
        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            string username = textBoxUser.Text.Trim();
            string email = textBoxEmail.Text.Trim();
            string code = textBoxCode.Text.Trim();
            string newPass = textBoxNewPass.Text;
            string confirmPass = textBoxConfirmPass.Text;

            if (string.IsNullOrEmpty(code) ||
                string.IsNullOrEmpty(newPass) ||
                string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mã xác minh và mật khẩu mới.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentOtp == null || _otpExpireTime == null)
            {
                MessageBox.Show("Bạn chưa yêu cầu mã OTP.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (DateTime.UtcNow > _otpExpireTime.Value)
            {
                MessageBox.Show("Mã OTP đã hết hạn. Vui lòng yêu cầu lại.",
                    "Hết hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(code, _currentOtp, StringComparison.Ordinal))
            {
                MessageBox.Show("Mã OTP không đúng.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass.Length < 4)
            {
                MessageBox.Show("Mật khẩu mới quá ngắn (ít nhất 4 ký tự).",
                    "Lưu ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonConfirm.Enabled = false;

            try
            {
                // --- Gọi API đổi mật khẩu: POST /account/change-password ---
                var payload = new
                {
                    Username = username,
                    NewPassword = newPass
                };

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage resp =
                    await httpClient.PostAsync("account/change-password", content);

                string respBody = await resp.Content.ReadAsStringAsync();
                string msg = ExtractMessage(respBody);

                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(msg) ? "Đổi mật khẩu thành công." : msg,
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(msg) ? "Đổi mật khẩu thất bại." : msg,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    buttonConfirm.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới máy chủ.\n" + ex.Message,
                    "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonConfirm.Enabled = true;
            }
        }

        //  HỦY / ĐÓNG FORM
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // lấy "message" từ JSON response backend
        private string ExtractMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("message", out var msgProp) &&
                    msgProp.ValueKind == JsonValueKind.String)
                {
                    return msgProp.GetString();
                }
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        // Helper: lấy Email từ JSON account trả về từ GET /account/{username}
        private string? ExtractEmail(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("Email", out var emailProp) &&
                    emailProp.ValueKind == JsonValueKind.String)
                {
                    return emailProp.GetString();
                }
            }
            catch
            {
                // ignore
            }
            return null;
        }

        private void FogotPass_Load(object sender, EventArgs e)
        {

        }
    }
}
