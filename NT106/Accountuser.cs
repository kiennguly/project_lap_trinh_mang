using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Accountuser : Form
    {
        private readonly S3ImageService _imageService = new S3ImageService();

        // API rank giống form Rank
        private const string RANK_API_BASE = "https://f1oj97uhee.execute-api.ap-southeast-1.amazonaws.com";
        private static readonly HttpClient http = new HttpClient();
        private readonly JsonSerializerOptions jsonOpt = new() { PropertyNameCaseInsensitive = true };

        private class RankItem
        {
            public string Username { get; set; } = "";
            public int Level { get; set; }
            public int Rank { get; set; }
        }

        private class GetResp
        {
            public bool ok { get; set; }
            public List<RankItem> ranking { get; set; } = new();
        }

        public Accountuser()
        {
            InitializeComponent();
        }

        // ===== LOAD FORM =====
        private async void Accountuser_Load(object sender, EventArgs e)
        {
            // Fill basic info từ AccountData
            txtUsername.Text = AccountData.Username ?? "";
            txtEmail.Text = AccountData.Email ?? "";
            txtLevel.Text = AccountData.Level.ToString();

            await LoadAvatarAsync();
            await LoadRankAsync();
        }

        // ===== AVATAR =====
        private async Task LoadAvatarAsync()
        {
            if (string.IsNullOrEmpty(AccountData.Username))
                return;

            // thử 2 dạng key khác nhau cho chắc
            string[] keysToTry =
            {
                $"avatars/avatars/{AccountData.Username}.png",
                $"avatars/{AccountData.Username}.png"
            };

            foreach (var key in keysToTry)
            {
                try
                {
                    var img = await _imageService.GetImageAsync(key);
                    if (img != null)
                    {
                        pictureAvatar.Image = img;
                        return;
                    }
                }
                catch
                {
                    // thử key tiếp theo
                }
            }
        }

        // ===== RANK =====
        private async Task LoadRankAsync()
        {
            string user = AccountData.Username ?? "";
            if (string.IsNullOrWhiteSpace(user))
            {
                txtRank.Text = "-";
                return;
            }

            txtRank.Text = "...";

            try
            {
                int topN = 100; // kiếm trong TOP 100
                string url = $"{RANK_API_BASE}/get?limit={topN}";
                var resp = await http.GetFromJsonAsync<GetResp>(url, jsonOpt);

                var list = resp?.ranking ?? new List<RankItem>();
                var mine = list.Find(r =>
                    string.Equals(r.Username, user, StringComparison.OrdinalIgnoreCase));

                if (mine != null)
                {
                    txtRank.Text = mine.Rank.ToString();
                }
                else
                {
                    txtRank.Text = $"Ngoài TOP {topN}";
                }
            }
            catch
            {
                txtRank.Text = "Lỗi tải rank";
            }
        }

        // ===== NÚT ĐÓNG =====
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e) => new ChangePass().Show();

        private void button1_Click(object sender, EventArgs e) => new giftcode().Show();

    }
}
