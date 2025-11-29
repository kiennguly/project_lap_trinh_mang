using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class GAMESOLO : Form
    {
        private NetworkManager _network;
        private bool _isHost;
        private string _roomId;

        private PictureBox _player, _opponent;
        private PictureBox _playerBullet, _opponentBullet;
        private System.Windows.Forms.Timer _gameTimer;

        private bool _goLeft, _goRight, _goUp, _goDown;
        private int _playerSpeed = 7;
        private int _bulletSpeed = 12;

        // Hướng bay theo trục Y: -1 = lên, +1 = xuống
        private int _playerBulletDir;
        private int _opponentBulletDir;

        private bool _gameEnded = false;
        private bool _paused = false;
        private int _stateTickCounter = 0;

        private string _localName;
        private string _opponentName = "Đối thủ";

        // HP
        private const int MAX_HP = 5;
        private int _playerHp = MAX_HP;
        private int _opponentHp = MAX_HP;

        private Label _hudYou, _hudEnemy;

        private Panel _hpYouBg, _hpYouFill;
        private Panel _hpEnemyBg, _hpEnemyFill;

        private Panel _pausePanel;
        private Button _btnResume, _btnQuit;

        // Ảnh gốc để vẽ đạn (không bị Dispose)
        private Bitmap _playerBulletBaseImg;
        private Bitmap _opponentBulletBaseImg;

        // Service lấy ảnh từ S3 (giữ lại nếu sau này dùng avatar ở góc)
        private readonly S3ImageService _s3 = new S3ImageService();

        // Trạng thái kết nối / trận
        private bool _opponentConnected = false;
        private bool _gameStarted = false;

        // ================== CONSTRUCTOR ==================

        public GAMESOLO(NetworkManager network, bool isHost, string roomId)
        {
            InitializeComponent();

            // DÙNG ĐÚNG NetworkManager TỪ ROOM, KHÔNG TẠO MỚI
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _isHost = isHost;
            _roomId = string.IsNullOrWhiteSpace(roomId) ? "SOLO" : roomId;

            _localName = string.IsNullOrEmpty(AccountData.Username)
                ? (_isHost ? "Host" : "Client")
                : AccountData.Username;

            this.Text = (_isHost ? "[HOST] " : "[CLIENT] ") + "Room: " + _roomId;
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            SetupGameObjects();
            SetupDirections();
            SetupHud();
            SetupPauseOverlay();
            SetupTimer();
            WireNetworkEvents();

        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_gameEnded && keyData == Keys.Space)
            {
                if (!_paused) FirePlayerBullet();
                return true;
            }
            if (keyData == Keys.Escape)
            {
                TogglePause();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ================== SETUP GAME OBJECTS ==================

        private void SetupGameObjects()
        {
            int w = Math.Max(800, this.ClientSize.Width);
            int h = Math.Max(600, this.ClientSize.Height);
            int ship = 96;   // máy bay to hơn

            // --- NỀN MAP PVP ---
            try
            {
                string bgPath = Path.Combine(Application.StartupPath, "bg_pvp.png");
                if (File.Exists(bgPath))
                {
                    this.BackgroundImage = Image.FromFile(bgPath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    this.BackColor = Color.Black;
                }
            }
            catch
            {
                this.BackColor = Color.Black;
            }

            // --- MÁY BAY ---
            _player = new PictureBox { Width = ship, Height = ship, BackColor = Color.Transparent };
            _opponent = new PictureBox { Width = ship, Height = ship, BackColor = Color.Transparent };

            Image hostPlane = null, clientPlane = null;

            try
            {
                string hostPlanePath = Path.Combine(Application.StartupPath, "host.png");
                string clientPlanePath = Path.Combine(Application.StartupPath, "client.png");

                if (File.Exists(hostPlanePath))
                    hostPlane = Image.FromFile(hostPlanePath);
                else
                    hostPlane = Properties.Resource.host;

                if (File.Exists(clientPlanePath))
                    clientPlane = Image.FromFile(clientPlanePath);
                else
                    clientPlane = Properties.Resource.client;
            }
            catch { }

            // ✓ ÉP trong suốt cho nền ảnh (ví dụ nền trắng)
            if (hostPlane != null)
            {
                Bitmap b = new Bitmap(hostPlane);
                b.MakeTransparent(Color.White);
                hostPlane = b;
            }

            if (clientPlane != null)
            {
                Bitmap b = new Bitmap(clientPlane);
                b.MakeTransparent(Color.White);
                clientPlane = b;
            }

            // Nếu không tìm thấy file, fallback plain màu
            if (hostPlane == null)
            {
                hostPlane = new Bitmap(ship, ship);
                using (var g = Graphics.FromImage(hostPlane))
                    g.Clear(Color.DeepSkyBlue);
            }
            if (clientPlane == null)
            {
                clientPlane = new Bitmap(ship, ship);
                using (var g = Graphics.FromImage(clientPlane))
                    g.Clear(Color.OrangeRed);
            }

            // Tạo 2 phiên bản quay lên / quay xuống
            Image hostUp = hostPlane;
            Image hostDown = (Image)hostPlane.Clone();
            hostDown.RotateFlip(RotateFlipType.Rotate180FlipNone);

            Image clientUp = clientPlane;
            Image clientDown = (Image)clientPlane.Clone();
            clientDown.RotateFlip(RotateFlipType.Rotate180FlipNone);

            // Host ở dưới, client ở trên.
            // Quy ước: máy bay dưới quay lên, máy bay trên quay xuống.
            if (_isHost)
            {
                // YOU (host) ở dưới -> quay lên
                _player.Left = (w - ship) / 2;
                _player.Top = h - ship - 70;
                _player.Image = hostUp;

                // ENEMY (client) ở trên -> quay xuống
                _opponent.Left = (w - ship) / 2;
                _opponent.Top = 70;
                _opponent.Image = clientDown;
            }
            else
            {
                // YOU (client) ở trên -> quay xuống
                _player.Left = (w - ship) / 2;
                _player.Top = 70;
                _player.Image = clientDown;

                // ENEMY (host) ở dưới -> quay lên
                _opponent.Left = (w - ship) / 2;
                _opponent.Top = h - ship - 70;
                _opponent.Image = hostUp;
            }

            _player.BackColor = Color.Transparent;
            _opponent.BackColor = Color.Transparent;
            _player.SizeMode = PictureBoxSizeMode.StretchImage;
            _opponent.SizeMode = PictureBoxSizeMode.StretchImage;

            Controls.Add(_player);
            Controls.Add(_opponent);

            // Đạn
            InitPlayerBulletSprite();
            InitOpponentBulletSprite();

            _player.BringToFront();
            _opponent.BringToFront();
            _playerBullet.BringToFront();
            _opponentBullet.BringToFront();

            try { btnExit.BringToFront(); } catch { }

            this.PreviewKeyDown += AnyControl_PreviewKeyDown;
            try { btnExit.PreviewKeyDown += AnyControl_PreviewKeyDown; } catch { }
            _player.PreviewKeyDown += AnyControl_PreviewKeyDown;
            _opponent.PreviewKeyDown += AnyControl_PreviewKeyDown;

            this.KeyDown += GAMESOLO_KeyDown;
            this.KeyUp += GAMESOLO_KeyUp;
        }

        private void SetupDirections()
        {
            // Host bắn lên, client bắn xuống
            if (_isHost)
            {
                _playerBulletDir = -1;   // host ở dưới -> bắn lên
                _opponentBulletDir = +1; // client ở trên -> bắn xuống
            }
            else
            {
                _playerBulletDir = +1;   // client ở trên -> bắn xuống
                _opponentBulletDir = -1; // host ở dưới -> bắn lên
            }
        }

        // ================== VẼ ĐẠN ==================

        private void InitPlayerBulletSprite()
        {
            _playerBullet = new PictureBox
            {
                Size = new Size(22, 72),
                BackColor = Color.Transparent,
                Visible = false
            };

            Bitmap bmp = new Bitmap(_playerBullet.Width, _playerBullet.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float centerX = _playerBullet.Width / 2f;

                int bodyWidth = 10;
                int bodyHeight = 32;
                int bodyX = (int)(centerX - bodyWidth / 2f);
                int bodyY = 8;
                Rectangle bodyRect = new Rectangle(bodyX, bodyY, bodyWidth, bodyHeight);

                using (var bodyBrush = new SolidBrush(Color.White))
                    g.FillRectangle(bodyBrush, bodyRect);
                using (var bodyPen = new Pen(Color.FromArgb(220, 180, 180, 180), 1f))
                    g.DrawRectangle(bodyPen, bodyRect);

                PointF tip = new PointF(centerX, 0);
                PointF leftBase = new PointF(bodyX, bodyY);
                PointF rightBase = new PointF(bodyX + bodyWidth, bodyY);
                PointF[] nose = { tip, leftBase, rightBase };
                using (var noseBrush = new SolidBrush(Color.OrangeRed))
                    g.FillPolygon(noseBrush, nose);

                Rectangle windowRect = new Rectangle(bodyX + 1, bodyY + 6, bodyWidth - 2, bodyWidth - 6);
                using (var windowBrush = new SolidBrush(Color.FromArgb(220, 80, 160, 255)))
                    g.FillEllipse(windowBrush, windowRect);

                using (var finBrush = new SolidBrush(Color.FromArgb(200, 0, 180, 255)))
                {
                    PointF[] leftFin =
                    {
                        new PointF(bodyX, bodyY + bodyHeight - 4),
                        new PointF(bodyX - 6, bodyY + bodyHeight + 6),
                        new PointF(bodyX, bodyY + bodyHeight + 3),
                    };
                    g.FillPolygon(finBrush, leftFin);

                    PointF[] rightFin =
                    {
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight - 4),
                        new PointF(bodyX + bodyWidth + 6, bodyY + bodyHeight + 6),
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight + 3),
                    };
                    g.FillPolygon(finBrush, rightFin);
                }

                int flameHeight = 24;
                Rectangle flameRect = new Rectangle(bodyX + 1, bodyY + bodyHeight, bodyWidth - 2, flameHeight);

                using (var flameBrush = new LinearGradientBrush(
                    new Point(flameRect.X, flameRect.Y),
                    new Point(flameRect.X, flameRect.Bottom),
                    Color.FromArgb(240, 0, 255, 255),
                    Color.FromArgb(0, 0, 255, 255)))
                {
                    g.FillRectangle(flameBrush, flameRect);
                }

                Rectangle glowRect = new Rectangle(
                    flameRect.X - 8,
                    flameRect.Bottom - 10,
                    flameRect.Width + 16,
                    20
                );
                using (var glowBrush = new SolidBrush(Color.FromArgb(120, 0, 200, 255)))
                    g.FillEllipse(glowBrush, glowRect);
            }

            _playerBulletBaseImg = bmp;
            _playerBullet.Image = (Bitmap)bmp.Clone();
            _playerBullet.SizeMode = PictureBoxSizeMode.Normal;

            Controls.Add(_playerBullet);
            _playerBullet.BringToFront();
        }

        private void InitOpponentBulletSprite()
        {
            _opponentBullet = new PictureBox
            {
                Size = new Size(22, 72),
                BackColor = Color.Transparent,
                Visible = false
            };

            Bitmap bmp = new Bitmap(_opponentBullet.Width, _opponentBullet.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float centerX = _opponentBullet.Width / 2f;

                int bodyWidth = 10;
                int bodyHeight = 32;
                int bodyX = (int)(centerX - bodyWidth / 2f);
                int bodyY = 8;
                Rectangle bodyRect = new Rectangle(bodyX, bodyY, bodyWidth, bodyHeight);

                using (var bodyBrush = new SolidBrush(Color.LightYellow))
                    g.FillRectangle(bodyBrush, bodyRect);
                using (var bodyPen = new Pen(Color.FromArgb(220, 200, 120, 120), 1f))
                    g.DrawRectangle(bodyPen, bodyRect);

                PointF tip = new PointF(centerX, 0);
                PointF leftBase = new PointF(bodyX, bodyY);
                PointF rightBase = new PointF(bodyX + bodyWidth, bodyY);
                PointF[] nose = { tip, leftBase, rightBase };
                using (var noseBrush = new SolidBrush(Color.Red))
                    g.FillPolygon(noseBrush, nose);

                Rectangle windowRect = new Rectangle(bodyX + 1, bodyY + 6, bodyWidth - 2, bodyWidth - 6);
                using (var windowBrush = new SolidBrush(Color.FromArgb(220, 255, 140, 80)))
                    g.FillEllipse(windowBrush, windowRect);

                using (var finBrush = new SolidBrush(Color.FromArgb(200, 255, 100, 0)))
                {
                    PointF[] leftFin =
                    {
                        new PointF(bodyX, bodyY + bodyHeight - 4),
                        new PointF(bodyX - 6, bodyY + bodyHeight + 6),
                        new PointF(bodyX, bodyY + bodyHeight + 3),
                    };
                    g.FillPolygon(finBrush, leftFin);

                    PointF[] rightFin =
                    {
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight - 4),
                        new PointF(bodyX + bodyWidth + 6, bodyY + bodyHeight + 6),
                        new PointF(bodyX + bodyWidth, bodyY + bodyHeight + 3),
                    };
                    g.FillPolygon(finBrush, rightFin);
                }

                int flameHeight = 24;
                Rectangle flameRect = new Rectangle(bodyX + 1, bodyY + bodyHeight, bodyWidth - 2, flameHeight);

                using (var flameBrush = new LinearGradientBrush(
                    new Point(flameRect.X, flameRect.Y),
                    new Point(flameRect.X, flameRect.Bottom),
                    Color.FromArgb(240, 255, 160, 0),
                    Color.FromArgb(0, 255, 0, 0)))
                {
                    g.FillRectangle(flameBrush, flameRect);
                }

                Rectangle glowRect = new Rectangle(
                    flameRect.X - 8,
                    flameRect.Bottom - 10,
                    flameRect.Width + 16,
                    20
                );
                using (var glowBrush = new SolidBrush(Color.FromArgb(120, 255, 80, 0)))
                    g.FillEllipse(glowBrush, glowRect);
            }

            _opponentBulletBaseImg = bmp;
            _opponentBullet.Image = (Bitmap)bmp.Clone();
            _opponentBullet.SizeMode = PictureBoxSizeMode.Normal;

            Controls.Add(_opponentBullet);
            _opponentBullet.BringToFront();
        }

        // ================== HUD & PAUSE ==================

        private void SetupHud()
        {
            _hudYou = new Label
            {
                AutoSize = true,
                ForeColor = Color.Lime,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            _hudEnemy = new Label
            {
                AutoSize = true,
                ForeColor = Color.Orange,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            int barWidth = 200;
            int barHeight = 16;

            _hpYouBg = new Panel
            {
                Width = barWidth,
                Height = barHeight,
                BackColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            _hpYouFill = new Panel
            {
                Height = barHeight - 2,
                Left = 1,
                Top = 1,
                BackColor = Color.Lime
            };
            _hpYouBg.Controls.Add(_hpYouFill);

            _hpEnemyBg = new Panel
            {
                Width = barWidth,
                Height = barHeight,
                BackColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            _hpEnemyFill = new Panel
            {
                Height = barHeight - 2,
                Left = 1,
                Top = 1,
                BackColor = Color.Lime
            };
            _hpEnemyBg.Controls.Add(_hpEnemyFill);

            int gap = 6;

            if (_isHost)
            {
                // YOU (host) ở dưới
                _hudYou.Left = 10;
                _hudYou.Top = ClientSize.Height - 60;
                _hpYouBg.Left = 10;
                _hpYouBg.Top = _hudYou.Bottom + gap;

                // ENEMY (client) ở trên
                _hudEnemy.Left = 10;
                _hudEnemy.Top = 10;
                _hpEnemyBg.Left = 10;
                _hpEnemyBg.Top = _hudEnemy.Bottom + gap;
            }
            else
            {
                // YOU (client) ở trên
                _hudYou.Left = 10;
                _hudYou.Top = 10;
                _hpYouBg.Left = 10;
                _hpYouBg.Top = _hudYou.Bottom + gap;

                // ENEMY (host) ở dưới
                _hudEnemy.Left = 10;
                _hudEnemy.Top = ClientSize.Height - 60;
                _hpEnemyBg.Left = 10;
                _hpEnemyBg.Top = _hudEnemy.Bottom + gap;
            }

            Controls.Add(_hudYou);
            Controls.Add(_hudEnemy);
            Controls.Add(_hpYouBg);
            Controls.Add(_hpEnemyBg);

            UpdateHud();
        }

        private void UpdateHud()
        {
            _hudYou.Text = _localName;
            _hudEnemy.Text = _opponentName;

            UpdateHpBar(_hpYouFill, _hpYouBg.Width, _playerHp);
            UpdateHpBar(_hpEnemyFill, _hpEnemyBg.Width, _opponentHp);
        }

        private void UpdateHpBar(Panel fill, int maxWidth, int hp)
        {
            int clamped = Math.Max(0, Math.Min(MAX_HP, hp));
            int w = (int)((maxWidth - 2) * (clamped / (float)MAX_HP));
            if (w < 0) w = 0;

            fill.Width = w;

            if (clamped >= 4) fill.BackColor = Color.Lime;
            else if (clamped >= 2) fill.BackColor = Color.Yellow;
            else fill.BackColor = Color.Red;
        }

        private void SetupPauseOverlay()
        {
            _pausePanel = new Panel
            {
                Visible = false,
                BackColor = Color.FromArgb(180, 0, 0, 0),
                Left = 0,
                Top = 0,
                Width = ClientSize.Width,
                Height = ClientSize.Height,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _btnResume = new Button { Text = "Tiếp tục (ESC)", Width = 160, Height = 40 };
            _btnQuit = new Button { Text = "Thoát trận", Width = 160, Height = 40, Top = 60 };
            _btnResume.Left = _btnQuit.Left = (ClientSize.Width - _btnResume.Width) / 2;
            _btnResume.Top = (ClientSize.Height - 100) / 2;

            _btnResume.Click += (s, e) => TogglePause();
            _btnQuit.Click += (s, e) => ConfirmExit();

            _pausePanel.Controls.Add(_btnResume);
            _pausePanel.Controls.Add(_btnQuit);
            Controls.Add(_pausePanel);
            _pausePanel.BringToFront();
        }

        private void TogglePause()
        {
            if (_gameEnded) return;
            _paused = !_paused;
            _pausePanel.Visible = _paused;
            if (_paused)
            {
                _gameTimer.Stop();
                lblStatusGame.Text = "Tạm dừng";
            }
            else
            {
                _gameTimer.Start();
                lblStatusGame.Text = _gameStarted ? "Đang chơi…" : "Đang chờ đối thủ…";
            }
        }

        private void SetupTimer()
        {
            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        // ================== NETWORK WIRING ==================

        private void WireNetworkEvents()
        {
            if (_network == null) return;

            _network.OnMessageReceived += msg =>
            {
                if (IsDisposed) return;
                if (InvokeRequired) BeginInvoke(new Action(() => ProcessNetworkMessage(msg)));
                else ProcessNetworkMessage(msg);
            };

            _network.OnDisconnected += () =>
            {
                if (IsDisposed) return;
                if (InvokeRequired) BeginInvoke(new Action(OnDisconnectedUI));
                else OnDisconnectedUI();
            };

            // Gửi chào để 2 bên biết tên nhau
            SafeSend(new { type = "hello", name = _localName });
            lblStatusGame.Text = "Đang chờ đối thủ…";
        }

        private void ProcessNetworkMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            if (msg == "START_GAME") return; // START_GAME xử lý ở Room, không cần trong đây

            try
            {
                using var doc = JsonDocument.Parse(msg);
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var tp)) return;
                string type = tp.GetString();

                switch (type)
                {
                    case "hello":
                        if (root.TryGetProperty("name", out var n))
                        {
                            _opponentConnected = true;

                            _opponentName = n.GetString() ?? _opponentName;
                            this.Text = (_isHost ? "[HOST] " : "[CLIENT] ") +
                                        "Room: " + _roomId + "  - vs " + _opponentName;
                            lblStatusGame.Text = "Đã kết nối với " + _opponentName;
                            SafeSend(new { type = "hello", name = _localName });
                            UpdateHud();
                        }
                        break;


                    case "state":
                        // khi lần đầu nhận state từ đối thủ mà chưa biết tên,
                        // mình yêu cầu nó gửi lại "hello"
                        if (!_opponentConnected)
                        {
                            _opponentConnected = true;
                            SafeSend(new { type = "hello_req" });
                        }

                        if (_opponent == null) return;
                        if (root.TryGetProperty("x", out var x) && root.TryGetProperty("y", out var y))
                        {
                            _opponent.Left = x.GetInt32();
                            _opponent.Top = y.GetInt32();

                            if (!_gameStarted)
                            {
                                _gameStarted = true;
                                lblStatusGame.Text = "Đang chơi…";
                            }
                        }
                        break;

                    case "shoot":
                        if (!_gameStarted)
                        {
                            _gameStarted = true;
                            lblStatusGame.Text = "Đang chơi…";
                        }
                        SpawnOpponentBullet();
                        break;

                    case "hp":
                        if (root.TryGetProperty("p", out var pProp) && root.TryGetProperty("o", out var oProp))
                        {
                            int pHp = pProp.GetInt32();
                            int oHp = oProp.GetInt32();

                            if (_isHost)
                            {
                                _playerHp = pHp;
                                _opponentHp = oHp;
                            }
                            else
                            {
                                _opponentHp = pHp; // host
                                _playerHp = oHp;   // local client
                            }

                            UpdateHud();
                        }
                        break;

                    case "result":
                        if (_gameEnded) return;
                        if (root.TryGetProperty("winner", out var wProp))
                        {
                            string winnerName = wProp.GetString();
                            bool youWin = string.Equals(winnerName, _localName,
                                StringComparison.OrdinalIgnoreCase);
                            EndGame(youWin, fromNetwork: true);
                        }
                        break;

                    case "quit":
                        if (_gameEnded) return;
                        _gameEnded = true;
                        try { _gameTimer?.Stop(); } catch { }
                        lblStatusGame.Text = "Đối thủ đã thoát trận.";
                        MarkRoomEnd();
                        MessageBox.Show(
                            "Đối thủ đã thoát trận. Quay lại lobby hoặc tạo phòng khác.",
                            "Thông báo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        Close();
                        break;
                }
            }
            catch
            {
                // bỏ qua message lỗi
            }
        }

        private void OnDisconnectedUI()
        {
            if (_gameEnded) return;
            _gameEnded = true;
            _gameTimer?.Stop();
            lblStatusGame.Text = "Mất kết nối.";
            MessageBox.Show("Kết nối bị ngắt. Quay lại lobby hoặc tạo phòng khác.",
                "Ngắt kết nối",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
        }

        private void SafeSend(object obj)
        {
            try
            {
                if (_network != null && _network.IsConnected)
                {
                    string json = JsonSerializer.Serialize(obj);
                    _network.Send(json);
                }
            }
            catch { }
        }

        // ================== ROOM STATUS (END) ==================

        private void MarkRoomEnd()
        {
            try
            {
                if (!string.IsNullOrEmpty(_roomId))
                {
                    _ = RoomApi.EndRoomAsync(_roomId);
                }
            }
            catch
            {
            }
        }

        // ================== GAME LOOP ==================

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_paused || _gameEnded || _player == null) return;

            int padding = 20;
            int midY = ClientSize.Height / 2;

            int minY = _isHost ? midY : padding;
            int maxY = _isHost ? (ClientSize.Height - _player.Height - padding)
                               : (midY - _player.Height);

            if (_goLeft && _player.Left > padding) _player.Left -= _playerSpeed;
            if (_goRight && _player.Right < ClientSize.Width - padding) _player.Left += _playerSpeed;
            if (_goUp && _player.Top > minY) _player.Top -= _playerSpeed;
            if (_goDown && _player.Top < maxY) _player.Top += _playerSpeed;

            if (_playerBullet.Visible)
            {
                _playerBullet.Top += _playerBulletDir * _bulletSpeed;
                if (_playerBullet.Top < 0 || _playerBullet.Top > ClientSize.Height)
                    _playerBullet.Visible = false;
            }

            if (_opponentBullet.Visible)
            {
                _opponentBullet.Top += _opponentBulletDir * _bulletSpeed;
                if (_opponentBullet.Top < 0 || _opponentBullet.Top > ClientSize.Height)
                    _opponentBullet.Visible = false;
            }

            if (_isHost)
            {
                if (_playerBullet.Visible && _opponent != null &&
                    _playerBullet.Bounds.IntersectsWith(_opponent.Bounds))
                {
                    _playerBullet.Visible = false;
                    ApplyHit(toOpponent: true);
                }

                if (_opponentBullet.Visible && _player != null &&
                    _opponentBullet.Bounds.IntersectsWith(_player.Bounds))
                {
                    _opponentBullet.Visible = false;
                    ApplyHit(toOpponent: false);
                }
            }

            _stateTickCounter++;
            if (_stateTickCounter % 2 == 0) SendPlayerState();
        }

        private void ApplyHit(bool toOpponent)
        {
            if (toOpponent) _opponentHp = Math.Max(0, _opponentHp - 1);
            else _playerHp = Math.Max(0, _playerHp - 1);

            UpdateHud();
            SafeSend(new { type = "hp", p = _playerHp, o = _opponentHp });

            if (_opponentHp <= 0 || _playerHp <= 0)
            {
                bool youWin = _opponentHp <= 0;
                EndGame(youWin, fromNetwork: false);
            }
        }

        private void SendPlayerState()
        {
            if (_network == null || !_network.IsConnected || _player == null) return;
            SafeSend(new { type = "state", x = _player.Left, y = _player.Top });
        }

        // ================== SHOOT & BULLETS ==================

        private void FirePlayerBullet()
        {
            if (_gameEnded || _player == null || _playerBullet == null) return;
            if (_playerBullet.Visible) return;

            if (_playerBulletBaseImg != null)
            {
                var img = _playerBulletBaseImg.Clone(
                    new Rectangle(0, 0, _playerBulletBaseImg.Width, _playerBulletBaseImg.Height),
                    _playerBulletBaseImg.PixelFormat);

                if (_playerBulletDir > 0)
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);

                _playerBullet.Image?.Dispose();
                _playerBullet.Image = img;
            }

            _playerBullet.Visible = true;
            _playerBullet.Left = _player.Left + _player.Width / 2 - _playerBullet.Width / 2;
            _playerBullet.Top = (_playerBulletDir < 0)
                ? (_player.Top - _playerBullet.Height)
                : _player.Bottom;

            if (!_gameStarted)
            {
                _gameStarted = true;
                lblStatusGame.Text = "Đang chơi…";
            }

            SafeSend(new { type = "shoot" });
        }

        private void SpawnOpponentBullet()
        {
            if (_opponent == null || _opponentBullet == null) return;
            if (_opponentBullet.Visible) return;

            if (_opponentBulletBaseImg != null)
            {
                var img = _opponentBulletBaseImg.Clone(
                    new Rectangle(0, 0, _opponentBulletBaseImg.Width, _opponentBulletBaseImg.Height),
                    _opponentBulletBaseImg.PixelFormat);

                if (_opponentBulletDir > 0)
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);

                _opponentBullet.Image?.Dispose();
                _opponentBullet.Image = img;
            }

            _opponentBullet.Visible = true;
            _opponentBullet.Left = _opponent.Left + _opponent.Width / 2 - _opponentBullet.Width / 2;
            _opponentBullet.Top = (_opponentBulletDir < 0)
                ? (_opponent.Top - _opponentBullet.Height)
                : _opponent.Bottom;
        }

        private void EndGame(bool youWin, bool fromNetwork)
        {
            if (_gameEnded) return;
            _gameEnded = true;
            _gameTimer?.Stop();
            _paused = false;
            _pausePanel.Visible = false;

            lblStatusGame.Text = "Trận đấu đã kết thúc";

            MarkRoomEnd();

            if (_isHost)
            {
                try
                {
                    string winner = youWin ? _localName : _opponentName;
                    string loser = youWin ? _opponentName : _localName;
                    Database.RecordMatchHistory(winner, loser);
                }
                catch { }
            }

            if (!fromNetwork)
                SafeSend(new { type = "result", winner = youWin ? _localName : _opponentName });

            MessageBox.Show(
                (youWin ? "🎉 Bạn THẮNG!\n" : "💥 Bạn THUA!\n") +
                $"{_localName} vs {_opponentName}",
                "Kết thúc trận đấu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            try
            {
                btnExit.Text = "Thoát trận";
                btnExit.Visible = true;
                btnExit.Enabled = true;
                btnExit.BringToFront();
            }
            catch { }
        }

        // ================== EVENTS / FORM ==================

        private void AnyControl_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        private void GAMESOLO_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_gameEnded || _paused) return;

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) _goLeft = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) _goRight = true;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W) _goUp = true;
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S) _goDown = true;
        }

        private void GAMESOLO_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) _goLeft = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) _goRight = false;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W) _goUp = false;
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S) _goDown = false;
        }

        private void btnExit_Click(object sender, EventArgs e) => ConfirmExit();

        private void ConfirmExit()
        {
            var ask = MessageBox.Show("Thoát trận và quay lại lobby?", "Thoát trận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ask == DialogResult.Yes)
            {
                // báo cho đối thủ mình thoát
                SafeSend(new { type = "quit", name = _localName });

                MarkRoomEnd();

                try { _gameTimer?.Stop(); } catch { }
                try
                {
                    (_network as IDisposable)?.Dispose();
                    _network = null;
                }
                catch { }

                Close();
            }
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            MarkRoomEnd();

            try
            {
                _gameTimer?.Stop();
                (_network as IDisposable)?.Dispose();
                _network = null;
            }
            catch { }
        }

        private void GAMESOLO_Load(object sender, EventArgs e)
        {
        }


        
    }
}
