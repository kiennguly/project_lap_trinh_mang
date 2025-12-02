using plan_fighting_super_start.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class GAMEBOSS : Form
    {
        // ==== Thông tin đạn boss để không phải parse string mỗi frame ====
        private sealed class BossBulletInfo
        {
            public int DirectionX;
            public int Speed;
        }

        // ==== Logic game ====
        private bool goLeft, goRight, shooting;
        private int playerSpeed = 8;
        private int bulletSpeed = 20;
        private int bossSpeed = 5;
        private int bossAttackTimer = 0;
        private int survivalTime = 90;

        private readonly Random rnd = new Random();
        private int frameCounter = 0;

        private const int BASE_DAMAGE = 10;
        private int playerDamage;

        // Bắn thưa hơn + giới hạn số đạn boss trên màn hình
        private int bossAttackFrequency = 120;
        private int maxBossBullets = 15;

        private bool isPaused = false;
        private bool gameEnded = false;

        // ⭐ Cooldown bắn đạn của player
        private int playerShootCooldown = 0;         // giảm dần mỗi tick
        private const int PLAYER_SHOOT_DELAY = 15;   // càng lớn bắn càng chậm

        // Dịch vụ lấy ảnh từ S3 (fallback nếu không có hình từ Menu)
        private readonly S3ImageService _imageService = new S3ImageService();

        // Ảnh máy bay đã chọn ở Menu (clone ra)
        private readonly Image? _planeImageFromMenu;

        // Danh sách control để không duyệt Controls mỗi lần tick
        private readonly List<PictureBox> _playerBullets = new List<PictureBox>();
        private readonly List<PictureBox> _bossBullets = new List<PictureBox>();
        private readonly List<PictureBox> _explosions = new List<PictureBox>();

        // Ảnh đạn tạo sẵn, tất cả bullet dùng chung → đỡ lag
        private readonly Bitmap _bossBulletBitmap;
        private readonly Bitmap _playerBulletBitmap;

        // ==== Constructor không tham số (nếu chỗ khác còn gọi) ====
        public GAMEBOSS() : this(null)
        {
        }

        // ==== Constructor chính: nhận hình máy bay từ Menu ====
        public GAMEBOSS(Image? planeImageFromMenu)
        {
            InitializeComponent();

            // vẽ mượt hơn
            this.DoubleBuffered = true;

            // clone image từ Menu để nếu Menu dispose vẫn an toàn
            _planeImageFromMenu = planeImageFromMenu != null
                ? (Image)planeImageFromMenu.Clone()
                : null;

            // tạo sẵn hình đạn boss & player
            _bossBulletBitmap = CreateBossBulletBitmap(10, 40);
            _playerBulletBitmap = CreatePlayerBulletBitmap(20, 60);
        }

        // ================== Tạo bitmap đạn một lần ==================

        private Bitmap CreateBossBulletBitmap(int w, int h)
        {
            var bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float centerX = w / 2f;

                // Glow vàng
                Rectangle glowRect = new Rectangle(0, 4, w, h - 4);
                using (var glowBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(glowRect.X, glowRect.Y),
                    new Point(glowRect.X, glowRect.Bottom),
                    Color.FromArgb(0, 255, 255, 0),
                    Color.FromArgb(220, 255, 210, 60)))
                {
                    g.FillEllipse(glowBrush, glowRect);
                }

                // Lõi đạn
                Rectangle coreRect = new Rectangle(
                    (int)(centerX - 2),
                    6,
                    4,
                    h - 16);
                using (var coreBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 220)))
                {
                    g.FillRectangle(coreBrush, coreRect);
                }

                // Đầu nhọn
                PointF p1 = new PointF(centerX, h);
                PointF p2 = new PointF(coreRect.Left - 3, coreRect.Bottom - 2);
                PointF p3 = new PointF(coreRect.Right + 3, coreRect.Bottom - 2);
                using (var tipBrush = new SolidBrush(Color.FromArgb(255, 255, 230, 140)))
                {
                    g.FillPolygon(tipBrush, new[] { p1, p2, p3 });
                }
            }
            return bmp;
        }

        private Bitmap CreatePlayerBulletBitmap(int w, int h)
        {
            var bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float centerX = w / 2f;

                // Thân tên lửa
                int bodyWidth = 8;
                int bodyHeight = 26;
                int bodyX = (int)(centerX - bodyWidth / 2f);
                int bodyY = 8;
                Rectangle bodyRect = new Rectangle(bodyX, bodyY, bodyWidth, bodyHeight);

                using (var bodyBrush = new SolidBrush(Color.White))
                    g.FillRectangle(bodyBrush, bodyRect);

                using (var bodyPen = new Pen(Color.FromArgb(200, 180, 180, 180), 1f))
                    g.DrawRectangle(bodyPen, bodyRect);

                // Đầu nhọn màu đỏ
                PointF tip = new PointF(centerX, 0);
                PointF leftBase = new PointF(bodyX, bodyY);
                PointF rightBase = new PointF(bodyX + bodyWidth, bodyY);
                using (var noseBrush = new SolidBrush(Color.OrangeRed))
                    g.FillPolygon(noseBrush, new[] { tip, leftBase, rightBase });

                // Cửa sổ xanh
                Rectangle windowRect = new Rectangle(bodyX + 1, bodyY + 6, bodyWidth - 2, bodyWidth - 4);
                using (var windowBrush = new SolidBrush(Color.FromArgb(220, 80, 160, 255)))
                    g.FillEllipse(windowBrush, windowRect);

                // Vây
                using (var finBrush = new SolidBrush(Color.FromArgb(200, 0, 180, 255)))
                {
                    PointF[] leftFin =
                    {
                        new PointF(bodyX, bodyY + bodyHeight - 4),
                        new PointF(bodyX - 5, bodyY + bodyHeight + 4),
                        new PointF(bodyX, bodyY + bodyHeight + 2),
                    };
                    g.FillPolygon(finBrush, leftFin);

                    PointF[] rightFin =
                    {
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight - 4),
                        new PointF(bodyX + bodyWidth + 5, bodyY + bodyHeight + 4),
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight + 2),
                    };
                    g.FillPolygon(finBrush, rightFin);
                }

                // Lửa xanh
                int flameHeight = 22;
                Rectangle flameRect = new Rectangle(bodyX + 1, bodyY + bodyHeight, bodyWidth - 2, flameHeight);
                using (var flameBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(flameRect.X, flameRect.Y),
                    new Point(flameRect.X, flameRect.Bottom),
                    Color.FromArgb(230, 0, 255, 255),
                    Color.FromArgb(0, 0, 255, 255)))
                {
                    g.FillRectangle(flameBrush, flameRect);
                }
            }
            return bmp;
        }

        //  Load skin máy bay 

        private async Task LoadPlaneSkinFromS3OrDefaultAsync()
        {
            try
            {
                string defaultPlanePath = Path.Combine(Application.StartupPath, "MayBay.png");

                if (string.IsNullOrEmpty(AccountData.PlaneSkin))
                {
                    if (File.Exists(defaultPlanePath))
                    {
                        player.Image = Image.FromFile(defaultPlanePath);
                        player.SizeMode = PictureBoxSizeMode.StretchImage;
                        player.BackColor = Color.Transparent;
                    }
                    return;
                }

                var img = await _imageService.GetImageAsync(AccountData.PlaneSkin);
                if (img != null)
                {
                    player.Image = img;
                    player.SizeMode = PictureBoxSizeMode.StretchImage;
                    player.BackColor = Color.Transparent;
                }
                else if (File.Exists(defaultPlanePath))
                {
                    player.Image = Image.FromFile(defaultPlanePath);
                    player.SizeMode = PictureBoxSizeMode.StretchImage;
                    player.BackColor = Color.Transparent;
                }
            }
            catch
            {
                string defaultPlanePath = Path.Combine(Application.StartupPath, "MayBay.png");
                if (File.Exists(defaultPlanePath))
                {
                    player.Image = Image.FromFile(defaultPlanePath);
                    player.SizeMode = PictureBoxSizeMode.StretchImage;
                    player.BackColor = Color.Transparent;
                }
            }
        }

        // Form Load 

        private async void Form4_Load(object sender, EventArgs e)
        {
            // Giữ lại PlaneSkin đang có (do Menu vừa đổi)
            string currentPlaneSkin = AccountData.PlaneSkin;

            if (!string.IsNullOrEmpty(AccountData.Username))
            {
                Database.LoadAccountData(AccountData.Username);
            }

            // Nếu backend chưa lưu PlaneSkin, giữ lại giá trị cũ
            if (!string.IsNullOrEmpty(currentPlaneSkin))
                AccountData.PlaneSkin = currentPlaneSkin;

            playerDamage = BASE_DAMAGE + AccountData.UpgradeDamage;

            playerHealthBar.Maximum = AccountData.UpgradeHP;
            playerHealthBar.Value = playerHealthBar.Maximum;
            playerHealthBar.ForeColor = Color.Lime;

            int currentBossMaxHealth = GetBossMaxHealth();
            bossHealthBar.Maximum = currentBossMaxHealth;
            bossHealthBar.Value = currentBossMaxHealth;
            bossHealthBar.ForeColor = Color.Red;

            survivalTime = 90;
            txtScore.Text = $"Gold: {AccountData.Gold}  Time: {survivalTime}  Level: {AccountData.Level}";

            // Ưu tiên dùng hình từ Menu (không gọi S3 nữa)
            if (_planeImageFromMenu != null)
            {
                player.Image = _planeImageFromMenu;
                player.SizeMode = PictureBoxSizeMode.StretchImage;
                player.BackColor = Color.Transparent;
            }
            else
            {
                await LoadPlaneSkinFromS3OrDefaultAsync();
            }

            gameTimer.Start();
            survivalTimer.Start();
        }

        private void GAMEBOSS_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
            this.Focus();
        }

        private int GetBossMaxHealth()
        {
            int level = Math.Max(1, AccountData.Level);
            double baseHp = 10000;      // HP level 1
            double growth = 1.3;        // +30% mỗi level
            double hp = baseHp * Math.Pow(growth, level - 1);
            return (int)hp;
        }

        // Vòng lặp game chính

        private void mainGameTimerEvent(object sender, EventArgs e)
        {
            if (isPaused) return;

            frameCounter++;
            txtScore.Text = $"Gold: {AccountData.Gold}  Time: {survivalTime}  Level: {AccountData.Level}";

            // Giảm cooldown bắn mỗi tick
            if (playerShootCooldown > 0)
                playerShootCooldown--;

            // Player movement
            if (goLeft && player.Left > 0)
                player.Left -= playerSpeed;
            if (goRight && player.Right < this.ClientSize.Width)
                player.Left += playerSpeed;

            // Boss movement
            boss.Left += bossSpeed;
            if (boss.Left < 0 || boss.Right > this.ClientSize.Width)
                bossSpeed = -bossSpeed;

            // Boss shooting
            bossAttackTimer++;
            if (bossAttackTimer > bossAttackFrequency)
            {
                bossAttackTimer = 0;
                ShootBossBulletFan();
            }

            // ===== Player bullets =====
            for (int i = _playerBullets.Count - 1; i >= 0; i--)
            {
                var pb = _playerBullets[i];
                pb.Top -= bulletSpeed;

                // Ra khỏi màn hình
                if (pb.Top < -pb.Height)
                {
                    RemoveBullet(_playerBullets, i, pb);
                    continue;
                }

                // Trúng boss
                if (pb.Bounds.IntersectsWith(boss.Bounds))
                {
                    bossHealthBar.Value = Math.Max(0, bossHealthBar.Value - playerDamage);
                    CreateExplosion(pb.Left, pb.Top, Color.Aqua);

                    RemoveBullet(_playerBullets, i, pb);

                    if (bossHealthBar.Value == 0)
                    {
                        ClearAllBulletsAndExplosions();
                        EndGame(true);
                        return;
                    }
                }
            }

            //  Boss bullets 
            for (int i = _bossBullets.Count - 1; i >= 0; i--)
            {
                var pb = _bossBullets[i];
                var info = (BossBulletInfo)pb.Tag;

                pb.Top += info.Speed;
                pb.Left += info.DirectionX * (info.Speed / 2);

                // Trúng player
                if (pb.Bounds.IntersectsWith(player.Bounds))
                {
                    playerHealthBar.Value = Math.Max(0, playerHealthBar.Value - 10);
                    if (playerHealthBar.Value < playerHealthBar.Maximum / 2)
                        playerHealthBar.ForeColor = Color.Yellow;
                    if (playerHealthBar.Value < playerHealthBar.Maximum / 4)
                        playerHealthBar.ForeColor = Color.Red;

                    CreateExplosion(pb.Left, pb.Top, Color.OrangeRed);

                    RemoveBullet(_bossBullets, i, pb);

                    if (playerHealthBar.Value == 0)
                    {
                        ClearAllBulletsAndExplosions();
                        EndGame(false);
                        return;
                    }
                }
                else if (pb.Top > this.ClientSize.Height + pb.Height ||
                         pb.Left < -pb.Width ||
                         pb.Right > this.ClientSize.Width + pb.Width)
                {
                    // ra khỏi màn hình
                    RemoveBullet(_bossBullets, i, pb);
                }
            }

            // Explosions 
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                var pb = _explosions[i];
                pb.Width += 4;
                pb.Height += 4;
                pb.Left -= 2;
                pb.Top -= 2;
                pb.BackColor = Color.FromArgb(
                    Math.Max(0, pb.BackColor.A - 20),
                    pb.BackColor.R, pb.BackColor.G, pb.BackColor.B);

                if (pb.BackColor.A <= 20)
                {
                    this.Controls.Remove(pb);
                    pb.Dispose();
                    _explosions.RemoveAt(i);
                }
            }

            // Điều chỉnh tần suất bắn theo số đạn boss hiện có
            bossAttackFrequency = _bossBullets.Count > maxBossBullets ? 200 : 80;
        }

        private void RemoveBullet(List<PictureBox> list, int index, PictureBox pb)
        {
            this.Controls.Remove(pb);
            pb.Dispose();
            list.RemoveAt(index);
        }

        private void ClearAllBulletsAndExplosions()
        {
            foreach (var pb in _playerBullets)
            {
                this.Controls.Remove(pb);
                pb.Dispose();
            }
            foreach (var pb in _bossBullets)
            {
                this.Controls.Remove(pb);
                pb.Dispose();
            }
            foreach (var pb in _explosions)
            {
                this.Controls.Remove(pb);
                pb.Dispose();
            }
            _playerBullets.Clear();
            _bossBullets.Clear();
            _explosions.Clear();
        }

        //  Tạo hiệu ứng & đạn 

        private void CreateExplosion(int x, int y, Color color)
        {
            PictureBox boom = new PictureBox
            {
                Size = new Size(16, 16),
                BackColor = Color.FromArgb(220, color.R, color.G, color.B),
                Left = x - 8,
                Top = y - 8
            };
            boom.BringToFront();
            _explosions.Add(boom);
            this.Controls.Add(boom);
        }

        private void ShootBossBulletFan()
        {
            int[] spreadDirections = {  -1, 0, 1 }; // 3 viên
            int baseSpeed = 20;

            foreach (int directionX in spreadDirections)
            {
                PictureBox bullet = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Normal,
                    Image = _bossBulletBitmap,
                    Size = _bossBulletBitmap.Size,
                    BackColor = Color.Transparent,
                    Left = boss.Left + boss.Width / 2 - _bossBulletBitmap.Width / 2,
                    Top = boss.Bottom - 5
                };

                int moveSpeed = baseSpeed + rnd.Next(-1, 2);
                bullet.Tag = new BossBulletInfo
                {
                    DirectionX = directionX,
                    Speed = moveSpeed
                };

                _bossBullets.Add(bullet);
                this.Controls.Add(bullet);
                bullet.BringToFront();
            }
        }

        private void ShootPlayerBullet()
        {
            PictureBox bullet = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Normal,
                Image = _playerBulletBitmap,
                Size = _playerBulletBitmap.Size,
                BackColor = Color.Transparent,
                Left = player.Left + player.Width / 2 - _playerBulletBitmap.Width / 2,
                Top = player.Top - _playerBulletBitmap.Height
            };

            _playerBullets.Add(bullet);
            this.Controls.Add(bullet);
            bullet.BringToFront();
        }

        // Survival timer 

        // Survival timer 
        private void survivalTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused || gameEnded) return;

            survivalTime--;

            if (survivalTime <= 0)
            {
                // không cho xuống âm để hiển thị cho đẹp
                survivalTime = 0;

                ClearAllBulletsAndExplosions();

                // Nếu boss còn máu -> thua, ngược lại (boss chết rồi) -> thắng
                if (bossHealthBar.Value > 0)
                {
                    EndGame(false);   // GAME OVER
                }
                else
                {
                    EndGame(true);    // WIN
                }
            }
        }


        // Kết thúc game 

        private void EndGame(bool win)
        {
            if (gameEnded) return;
            gameEnded = true;

            gameTimer.Stop();
            survivalTimer.Stop();
            isPaused = false;

            pausePanel.Visible = false;

            ClearAllBulletsAndExplosions();

            if (win)
            {
                AccountData.Gold += 200;
                AccountData.Level++;
            }
            else
            {
                AccountData.Gold += 50;
            }

            try { Database.UpdateAccountData(); } catch { }

            txtScore.Text =
                $"Gold: {AccountData.Gold}  Time: {survivalTime}  Level: {AccountData.Level}" +
                (win ? " - WIN!" : " - GAME OVER!");

            buttonExit.Text = "Thoát về menu";
            buttonExit.Visible = true;
        }

        // Pause / Resume 

        private void PauseGame()
        {
            if (isPaused || gameEnded) return;

            isPaused = true;
            gameTimer.Stop();
            survivalTimer.Stop();
            pausePanel.Visible = true;
            pauseTextLabel.Text = "⏸ TẠM DỪNG";
        }

        private void ResumeGame()
        {
            if (!isPaused || gameEnded) return;

            isPaused = false;
            gameTimer.Start();
            survivalTimer.Start();
            pausePanel.Visible = false;
        }

        private void btnResume_Click(object sender, EventArgs e) => ResumeGame();

        private void btnPauseExit_Click(object sender, EventArgs e)
        {
            if (gameEnded) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn thoát trận và quay về Menu?\nBạn sẽ không nhận thêm vàng cho trận này.",
                "Thoát trận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try { Database.UpdateAccountData(); } catch { }
            this.Close();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            if (!gameEnded)
            {
                try { Database.UpdateAccountData(); } catch { }
            }
            this.Close();
        }

        //  Input & misc 

        private void PlayHitSound() { }
        private void PlayLoseSound() { }
        private void PlayWinSound() { }

        private void keyisdown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) goLeft = true;
            if (e.KeyCode == Keys.Right) goRight = true;

            //  Bắn đạn theo cooldown
            if (e.KeyCode == Keys.Space && !isPaused && !gameEnded)
            {
                if (playerShootCooldown == 0)
                {
                    ShootPlayerBullet();
                    playerShootCooldown = PLAYER_SHOOT_DELAY;
                }
            }

            if (e.KeyCode == Keys.P)
            {
                if (!isPaused) PauseGame();
                else ResumeGame();
            }
        }

        private void keyisup(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) goLeft = false;
            if (e.KeyCode == Keys.Right) goRight = false;
            if (e.KeyCode == Keys.Space) shooting = false;
        }

        private void txtScore_Click(object sender, EventArgs e) { }

        private void GAMEBOSS_FormClosed(object sender, FormClosedEventArgs e)
        {
            _planeImageFromMenu?.Dispose();
            _bossBulletBitmap.Dispose();
            _playerBulletBitmap.Dispose();
        }

        private void boss_Click(object sender, EventArgs e) { }
    }
}
