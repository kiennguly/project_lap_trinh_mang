using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;   // dùng NAudio để phát nhạc

namespace plan_fighting_super_start
{
    public partial class Menu : Form
    {
        // ⭐ player nhạc nền với NAudio
        private IWavePlayer? waveOut;
        private AudioFileReader? audioFile;

        // ⭐ dịch vụ ảnh S3
        private readonly S3ImageService _imageService = new S3ImageService();

        // ⭐ dịch vụ đổi máy bay (S3)
        private readonly DoiMayBayService _doiMayBayService = new DoiMayBayService();
        private int _currentPlaneIndex = 0;

        // ===== Màu UI =====
        private readonly Color Teal = Color.FromArgb(0, 192, 192);
        private readonly Color BgDark = Color.FromArgb(10, 15, 30);
        private readonly Color BgButton = Color.FromArgb(15, 25, 45);

        public Menu()
        {
            InitializeComponent();

            // đăng ký sự kiện để dọn nhạc & set offline khi đóng form
            this.FormClosing += Menu_FormClosing;

            // khởi tạo nhạc nền
            InitBackgroundMusic();
        }

        // ===== Nhạc nền bossgame.mp3 dùng NAudio =====
        private void InitBackgroundMusic()
        {
            try
            {
                string mp3Path = Path.Combine(Application.StartupPath, "bossgame.mp3");

                if (!File.Exists(mp3Path))
                {
                    // Không có file thì thôi
                    return;
                }

                waveOut = new WaveOutEvent();
                audioFile = new AudioFileReader(mp3Path);

                waveOut.Init(audioFile);
                waveOut.Play();

                // Loop nhạc
                waveOut.PlaybackStopped += (s, e) =>
                {
                    if (audioFile != null && waveOut != null)
                    {
                        audioFile.Position = 0;
                        waveOut.Play();
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không phát được nhạc nền: " + ex.Message,
                    "Lỗi nhạc nền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Dừng nhạc, giải phóng + set Offline khi đóng form
        private void Menu_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // dọn nhạc
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }

                if (audioFile != null)
                {
                    audioFile.Dispose();
                    audioFile = null;
                }
            }
            catch
            {
            }

            // 🔹 set Online = false khi thoát Menu (fire-and-forget)
            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                {
                    _ = Database.SetOnlineStatusAsync(AccountData.Username, false);
                }
            }
            catch
            {
            }
        }

        // ===== Hàm dùng chung để load dữ liệu và cập nhật UI =====
        private void RefreshAccountDataAndUI()
        {
            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                {
                    Database.LoadAccountData(AccountData.Username);
                }
            }
            catch { }

            if (textBoxGold != null) textBoxGold.Text = AccountData.Gold.ToString();
            if (textBox1 != null) textBox1.Text = AccountData.UpgradeHP.ToString();
            if (textBox2 != null) textBox2.Text = AccountData.UpgradeDamage.ToString();
            if (textBox3 != null) textBox3.Text = AccountData.Level.ToString();
        }

        // 🔹 Load avatar từ S3: avatars/{username}.png
        private async void LoadAvatarAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(AccountData.Username)) return;

                string key = $"avatars/avatars/{AccountData.Username}.png";
                var img = await _imageService.GetImageAsync(key);

                if (pictureBoxAvatar != null && img != null)
                {
                    pictureBoxAvatar.Image = img;
                    pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch
            {
                // ignore lỗi avatar
            }
        }

        // Khởi tạo chỉ số máy bay từ DB
        private void InitPlaneIndexFromAccount()
        {
            if (string.IsNullOrEmpty(AccountData.PlaneSkin))
            {
                _currentPlaneIndex = 0;
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(AccountData.PlaneSkin); // plane3
            string digits = string.Empty;
            foreach (char c in fileName)
            {
                if (char.IsDigit(c)) digits += c;
            }

            if (int.TryParse(digits, out int idx) && idx >= 1 && idx <= 5)
                _currentPlaneIndex = idx;
            else
                _currentPlaneIndex = 0;
        }

        // Sự kiện load form
        private void Form3_Load(object sender, EventArgs e)
        {
            this.BackColor = BgDark;

            if (labelWelcome != null)
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                    labelWelcome.Text = $"Xin chào {AccountData.Username}";
                else
                    labelWelcome.Text = "Xin chào";
            }

            RefreshAccountDataAndUI();
            LoadAvatarAsync();
            InitPlaneIndexFromAccount();

            if (buttonPlay != null) SetGameButton(buttonPlay);
            if (buttonUpgradeHP != null) SetGameButton(buttonUpgradeHP);
            if (buttonUpgradeDamage != null) SetGameButton(buttonUpgradeDamage);
            if (buttonExit != null) SetGameButton(buttonExit);
            if (button1 != null) SetGameButton(button1);
            if (button2 != null) SetGameButton(button2);
            if (button3 != null) SetGameButton(button3);
            if (button4 != null) SetGameButton(button4);
            if (button5 != null) SetGameButton(button5);

            if (textBoxGold != null) SetStatTextBox(textBoxGold);
            if (textBox1 != null) SetStatTextBox(textBox1);
            if (textBox2 != null) SetStatTextBox(textBox2);
            if (textBox3 != null) SetStatTextBox(textBox3);

            if (labelWelcome != null) SetHeaderLabel(labelWelcome);
            if (label1 != null) SetInfoLabel(label1);
            if (label2 != null) SetInfoLabel(label2);
            if (label3 != null) SetInfoLabel(label3);
            if (label4 != null) SetInfoLabel(label4);
        }

        // ===== Helpers: chỉ UI =====
        private void SetGameButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Teal;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 120, 140);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 100);
            button.BackColor = BgButton;
            button.ForeColor = Teal;
            button.UseVisualStyleBackColor = false;

            button.MouseEnter += (_, __) =>
            {
                button.BackColor = Teal;
                button.ForeColor = Color.Black;
            };

            button.MouseLeave += (_, __) =>
            {
                button.BackColor = BgButton;
                button.ForeColor = Teal;
            };
        }

        private void SetHeaderLabel(Label label)
        {
            label.BackColor = Color.Transparent;
            label.ForeColor = Teal;
            label.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            label.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void SetInfoLabel(Label label)
        {
            label.BackColor = Color.Transparent;
            label.ForeColor = Teal;
            label.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        private void SetStatTextBox(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.FromArgb(15, 22, 45);
            textBox.ForeColor = Teal;
            textBox.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            textBox.ReadOnly = true;
            textBox.TextAlign = HorizontalAlignment.Center;
        }

        // ====== Handlers nút bấm ======
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            try
            {
                Image? planeImg = pictureBoxPlane?.Image;

                using (var form = new GAMEBOSS(planeImg))
                {
                    form.ShowDialog(this);
                }

                RefreshAccountDataAndUI();
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

                if (textBoxGold != null) textBoxGold.Text = AccountData.Gold.ToString();
                if (textBox1 != null) textBox1.Text = AccountData.UpgradeHP.ToString();

                try { Database.UpdateAccountData(); } catch { }
            }
            else
            {
                MessageBox.Show("Không đủ vàng để nâng HP!");
            }
        }

        private void buttonUpgradeDamage_Click(object sender, EventArgs e)
        {
            if (AccountData.Gold >= 15)
            {
                AccountData.Gold -= 15;
                AccountData.UpgradeDamage += 5;

                if (textBoxGold != null) textBoxGold.Text = AccountData.Gold.ToString();
                if (textBox2 != null) textBox2.Text = AccountData.UpgradeDamage.ToString();

                try { Database.UpdateAccountData(); } catch { }
            }
            else
            {
                MessageBox.Show("Không đủ vàng để nâng Damage!");
            }
        }

        private async void buttonExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(AccountData.Username))
                {
                    // Chờ set Online = false xong
                    await Database.SetOnlineStatusAsync(AccountData.Username, false);
                }
            }
            catch
            {
                // bỏ qua lỗi, vẫn thoát game
            }

            Application.Exit();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var form = new Room();
            form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new Rank();
            form.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var form = new ChangePass();
            form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var form = new Reward();
            form.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var form = new giftcode();
            form.Show();
        }

        // ⭐ NÚT ĐỔI MÁY BAY
        private async void buttonDoiMayBay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập trước khi đổi máy bay!");
                return;
            }

            if (_currentPlaneIndex == 0)
                _currentPlaneIndex = 1;
            else
            {
                _currentPlaneIndex++;
                if (_currentPlaneIndex > 5)
                    _currentPlaneIndex = 1;
            }

            try
            {
                var (img, key) = await _doiMayBayService.DoiMayBayAsync(_currentPlaneIndex);

                if (img != null && pictureBoxPlane != null)
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
                else
                {
                    MessageBox.Show("Không nhận được key máy bay từ server!", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi máy bay thất bại: " + ex.Message, "Lỗi");
            }
        }
    }
}
