using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class giftcode : Form
    {
        // URL API Gateway trỏ tới Lambda RedeemGiftcodeFunction
        // THAY chuỗi dưới bằng URL thật của bạn
        private const string GiftcodeApiUrl =
            "https://ueg0kxfq34.execute-api.ap-southeast-1.amazonaws.com/default/RedeemGiftcodeFunction";


        // HttpClient dùng chung
        private static readonly HttpClient httpClient = new HttpClient();

        // Options cho JSON
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Kết quả trả về từ Lambda
        private class RedeemResult
        {
            public bool Ok { get; set; }
            public string Message { get; set; } = "";
            public int GoldAdded { get; set; }
            public int DamageAdded { get; set; }
            public int NewGold { get; set; }
        }

        public giftcode()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Không cần xử lý gì cũng được
        }

        // Nút "Đổi" / "Redeem"
        private async void button1_Click(object sender, EventArgs e)
        {
            string code = textBox1.Text.Trim();
            string username = AccountData.Username ?? string.Empty;

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show(
                    "Vui lòng nhập giftcode.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                textBox1.Focus();
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show(
                    "Bạn cần đăng nhập trước khi nhập giftcode.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            button1.Enabled = false;

            try
            {
                // Gọi API giftcode
                var result = await RedeemGiftcodeAsync(username, code);

                if (!result.Ok)
                {
                    // Trường hợp code sai / đã dùng rồi...
                    MessageBox.Show(
                        result.Message ?? "Đổi giftcode thất bại.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                // Cập nhật dữ liệu local
                AccountData.Gold = result.NewGold;
                AccountData.UpgradeDamage += result.DamageAdded;

                MessageBox.Show(
                    $"Đổi giftcode thành công!\n\n" +
                    $"+{result.GoldAdded} vàng\n" +
                    $"+{result.DamageAdded} sát thương",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                textBox1.Clear();
                textBox1.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi gọi API giftcode:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        // Hàm gọi API Gateway + Lambda
        private static async Task<RedeemResult> RedeemGiftcodeAsync(string username, string code)
        {
            var payload = new
            {
                username = username,
                code = code
            };

            string json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(GiftcodeApiUrl, content);
            string respString = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RedeemResult>(respString, JsonOptions);
            return result ?? new RedeemResult
            {
                Ok = false,
                Message = "Không đọc được phản hồi từ server"
            };
        }
    }
}
