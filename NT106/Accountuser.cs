using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        // ===== Badge đỏ ở nút Friend =====
        private Label _friendBadge;

        // API rank giống form Rank
        private const string RANK_API_BASE = "https://f1oj97uhee.execute-api.ap-southeast-1.amazonaws.com";
        private static readonly HttpClient http = new HttpClient();
        private readonly JsonSerializerOptions jsonOpt = new() { PropertyNameCaseInsensitive = true };

        private class RankItem { public string Username { get; set; } = ""; public int Level { get; set; } public int Rank { get; set; } }
        private class GetResp { public bool ok { get; set; } public List<RankItem> ranking { get; set; } = new(); }

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

            // Phóng to chữ “Friend” và canh đều hàng nút phía dưới
            if (button2 != null)
            {
                button2.Font = new Font(button2.Font.FontFamily,
                                        Math.Max(12f, button2.Font.Size + 2f),
                                        FontStyle.Bold);
            }
            LayoutBottomButtons();
            this.Resize += (_, __) => LayoutBottomButtons();

            await LoadAvatarAsync();
            await LoadRankAsync();

            // Gắn badge & load số pending ban đầu
            AttachFriendBadge();
            await RefreshFriendBadgeAsync();
        }

        // ===== AVATAR =====
        private async Task LoadAvatarAsync()
        {
            if (string.IsNullOrEmpty(AccountData.Username))
                return;

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
                catch { }
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
                int topN = 100;
                string url = $"{RANK_API_BASE}/get?limit={topN}";
                var resp = await http.GetFromJsonAsync<GetResp>(url, jsonOpt);

                var list = resp?.ranking ?? new List<RankItem>();
                var mine = list.Find(r =>
                    string.Equals(r.Username, user, StringComparison.OrdinalIgnoreCase));

                txtRank.Text = mine != null ? mine.Rank.ToString() : $"Ngoài TOP {topN}";
            }
            catch
            {
                txtRank.Text = "Lỗi tải rank";
            }
        }

        // ===== Canh đều 4 nút dưới (Đổi Pass – Đóng – GiftCode – Friend) =====
        private void LayoutBottomButtons()
        {
            var btns = new List<Button>();
            if (button3 != null) btns.Add(button3); // Đổi Pass
            if (btnClose != null) btns.Add(btnClose);
            if (button1 != null) btns.Add(button1); // GiftCode
            if (button2 != null) btns.Add(button2); // Friend
            if (btns.Count == 0) return;

            int y = btns.Min(b => b.Top);
            int margin = 24;
            int totalW = btns.Sum(b => b.Width);
            int gap = Math.Max(12, (ClientSize.Width - totalW - margin * 2) / (btns.Count + 1));

            int x = margin + gap;
            foreach (var b in btns)
            {
                b.Top = y;
                b.Left = x;
                x += b.Width + gap;
            }

            PositionFriendBadge();
        }

        // ===== Badge đỏ trên nút Friend =====
        private void AttachFriendBadge()
        {
            if (_friendBadge != null || button2 == null) return;

            _friendBadge = new Label
            {
                AutoSize = false,
                Size = new Size(22, 22),
                BackColor = Color.FromArgb(230, 220, 20, 20),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = false
            };
            _friendBadge.Parent = button2.Parent;

            var gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, _friendBadge.Width, _friendBadge.Height);
            _friendBadge.Region = new Region(gp);

            PositionFriendBadge();

            button2.LocationChanged += (_, __) => PositionFriendBadge();
            button2.SizeChanged += (_, __) => PositionFriendBadge();
        }

        private void PositionFriendBadge()
        {
            if (_friendBadge == null || button2 == null) return;
            _friendBadge.Left = button2.Right - (_friendBadge.Width / 2);
            _friendBadge.Top = button2.Top - (_friendBadge.Height / 2);
            _friendBadge.BringToFront();
        }

        private async Task RefreshFriendBadgeAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(AccountData.Username))
                {
                    UpdateFriendBadge(0);
                    return;
                }

                var list = await Database.GetFriendListAsync(AccountData.Username);
                int pending = list.Count(x => string.Equals(x.Status, "pending", StringComparison.OrdinalIgnoreCase));
                UpdateFriendBadge(pending);
            }
            catch
            {
                UpdateFriendBadge(0);
            }
        }

        private void UpdateFriendBadge(int count)
        {
            if (_friendBadge == null) return;

            if (count <= 0)
            {
                _friendBadge.Visible = false;
            }
            else
            {
                _friendBadge.Text = count > 99 ? "99+" : count.ToString();
                _friendBadge.Visible = true;
                _friendBadge.BringToFront();
            }
        }

        // ===== NÚT =====
        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void button3_Click(object sender, EventArgs e)
        {
            Hide();
            using (var form = new ChangePass()) form.ShowDialog();
            Show();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Hide();
            using (var form = new giftcode()) form.ShowDialog();
            Show();
            await RefreshFriendBadgeAsync();  // có thể có thay đổi ở chỗ khác → làm mới badge
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            Hide();
            using (var form = new Friend()) form.ShowDialog();
            Show();
            await RefreshFriendBadgeAsync();  // sau khi đóng Friend → cập nhật lại badge
        }
    }
}
