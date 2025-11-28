using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;

namespace plan_fighting_super_start
{
    public partial class Menu : Form
    {
        private IWavePlayer? waveOut;
        private AudioFileReader? audioFile;

        private readonly S3ImageService _imageService = new S3ImageService();
        private readonly DoiMayBayService _doiMayBayService = new DoiMayBayService();
        private int _currentPlaneIndex = 0;

        // Màu chủ đạo
        private readonly Color Teal = Color.FromArgb(0, 192, 192);
        private readonly Color BgDark = Color.FromArgb(10, 15, 30);
        private readonly Color BgButton = Color.FromArgb(15, 25, 45);

        public Menu()
        {
            InitializeComponent();
            this.FormClosing += Menu_FormClosing;
            InitBackgroundMusic();
        }

        private void InitBackgroundMusic()
        {
            try
            {
                string mp3Path = Path.Combine(Application.StartupPath, "bossgame.mp3");
                if (!File.Exists(mp3Path)) return;

                waveOut = new WaveOutEvent();
                audioFile = new AudioFileReader(mp3Path);
                waveOut.Init(audioFile);
                waveOut.Play();
                waveOut.PlaybackStopped += (_, __) =>
                {
                    if (audioFile != null && waveOut != null)
                    {
                        audioFile.Position = 0;
                        waveOut.Play();
                    }
                };
            }
            catch { }
        }

        private void Menu_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try { waveOut?.Stop(); } catch { }
            try { waveOut?.Dispose(); waveOut = null; } catch { }
            try { audioFile?.Dispose(); audioFile = null; } catch { }

            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                    _ = Database.SetOnlineStatusAsync(AccountData.Username, false);
            }
            catch { }
        }

        //  UI helpers 
        private void StylePrimaryButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Teal;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 120, 140);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 100);
            b.BackColor = BgButton;
            b.ForeColor = Teal;
            b.UseVisualStyleBackColor = false;
        }

        private void StyleAccent(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Teal;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 150, 160);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 120, 140);
            b.BackColor = BgButton;
            b.ForeColor = Color.FromArgb(0, 255, 255);
            b.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        }

        private void StyleSmallLabel(Label l)
        {
            l.BackColor = Color.Transparent;
            l.ForeColor = Teal;
            l.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        private void StyleStat(TextBox t)
        {
            t.BackColor = Color.FromArgb(15, 22, 45);
            t.BorderStyle = BorderStyle.FixedSingle;
            t.ForeColor = Teal;
            t.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            t.ReadOnly = true;
            t.TextAlign = HorizontalAlignment.Center;
        }

        private void StyleHeader(Label l)
        {
            l.BackColor = Color.Transparent;
            l.ForeColor = Teal;
            l.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            l.TextAlign = ContentAlignment.MiddleCenter;
        }

        // Data/UI

        private void UpdateStatsUI()
        {
            textBoxGold.Text = AccountData.Gold.ToString();
            textBox1.Text = AccountData.UpgradeHP.ToString();
            textBox2.Text = AccountData.UpgradeDamage.ToString();
            textBox3.Text = AccountData.Level.ToString();
        }

        
        private void ReloadAccountFromServer()
        {
            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                    Database.LoadAccountData(AccountData.Username);
            }
            catch { }

            UpdateStatsUI();
        }

        private async void LoadAvatarAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(AccountData.Username)) return;
                string key = $"avatars/avatars/{AccountData.Username}.png";
                var img = await _imageService.GetImageAsync(key);
                if (img != null)
                {
                    pictureBoxAvatar.Image = img;
                    pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
        }

        private void InitPlaneIndexFromAccount()
        {
            if (string.IsNullOrEmpty(AccountData.PlaneSkin)) { _currentPlaneIndex = 0; return; }
            string name = Path.GetFileNameWithoutExtension(AccountData.PlaneSkin);
            string digits = "";
            foreach (var c in name) if (char.IsDigit(c)) digits += c;
            if (int.TryParse(digits, out int idx) && idx >= 1 && idx <= 5) _currentPlaneIndex = idx;
            else _currentPlaneIndex = 0;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.BackColor = BgDark;

            labelWelcome.Text = string.IsNullOrEmpty(AccountData.Username)
                ? "Xin chào"
                : $"Xin chào {AccountData.Username}";

            // style
            StyleHeader(labelWelcome);

            

            foreach (var l in new[] { label1, label2, label3, label4 })
                StyleSmallLabel(l);

            foreach (var t in new[] { textBoxGold, textBox1, textBox2, textBox3 })
                StyleStat(t);

            StyleAccent(buttonDoiMayBay);

            // load từ server rồi vẽ UI
            ReloadAccountFromServer();
            LoadAvatarAsync();
            InitPlaneIndexFromAccount();
        }

        // ==== Buttons ====
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBoxPlane.Image == null)
                {
                    MessageBox.Show("Chưa chọn máy bay!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Ẩn Menu
                this.Hide();

                // Mở form GAMEBOSS dạng modal
                using (var form = new GAMEBOSS(pictureBoxPlane.Image))
                {
                    form.ShowDialog();   // không cần truyền this
                }

                // Hiện lại Menu sau khi game đóng
                this.Show();

                // Sau khi chơi xong reload lại từ server nếu logic game có cập nhật vàng/lv
                ReloadAccountFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được chế độ chơi: " + ex.Message);
            }
        }


        private void buttonUpgradeHP_Click(object sender, EventArgs e)
        {
            if (AccountData.Gold >= 10)
            {
                AccountData.Gold -= 10;
                AccountData.UpgradeHP += 20;

                // Chỉ update UI theo dữ liệu mới
                UpdateStatsUI();

                // Rồi mới cập nhật lên backend
                try { Database.UpdateAccountData(); } catch { }
            }
            else MessageBox.Show("Không đủ vàng để nâng HP!");
        }

        private void buttonUpgradeDamage_Click(object sender, EventArgs e)
        {
            if (AccountData.Gold >= 15)
            {
                AccountData.Gold -= 15;
                AccountData.UpgradeDamage += 5;

                // Chỉ update UI theo dữ liệu mới
                UpdateStatsUI();

                // Rồi mới cập nhật lên backend
                try { Database.UpdateAccountData(); } catch { }
            }
            else MessageBox.Show("Không đủ vàng để nâng Damage!");
        }

        private async void buttonExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                    await Database.SetOnlineStatusAsync(AccountData.Username, false);
            }
            catch { }
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (Room gc = new Room())
            {
                gc.ShowDialog();
            }
            this.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (Rank gc = new Rank())
            {
                gc.ShowDialog();
            }
            this.Show();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (Reward gc = new Reward())
            {
                gc.ShowDialog();
            }
            this.Show();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (Accountuser gc = new Accountuser())
            {
                gc.ShowDialog();
            }
            this.Show();
        }

        private async void buttonDoiMayBay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập trước khi đổi máy bay!");
                return;
            }

            _currentPlaneIndex = (_currentPlaneIndex == 0) ? 1 : _currentPlaneIndex + 1;
            if (_currentPlaneIndex > 5) _currentPlaneIndex = 1;

            try
            {
                var (img, key) = await _doiMayBayService.DoiMayBayAsync(_currentPlaneIndex);
                if (img != null)
                {
                    pictureBoxPlane.Image = img;
                    pictureBoxPlane.SizeMode = PictureBoxSizeMode.Zoom;
                }
                if (!string.IsNullOrEmpty(key))
                {
                    AccountData.PlaneSkin = key;
                    try { Database.UpdateAccountData(); } catch { }
                    MessageBox.Show("Đã đổi máy bay!", "Thông báo");
                }
                else MessageBox.Show("Không nhận được key máy bay từ server!", "Lỗi");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi máy bay thất bại: " + ex.Message, "Lỗi");
            }
        }

        private void labelWelcome_Click(object sender, EventArgs e)
        {

        }
    }
}
