using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Room : Form
    {
        private NetworkManager networkManager;
        private LANBroadcast lanBroadcast;
        private ChatSanhLAN chatSanh;

        private bool isHost;
        private string currentRoomId;

        private const int GAME_PORT = 9000;

        private bool gameStarted = false;
        private bool shuttingDown = false;
        private volatile bool _connectedToRoom = false;

        // Hàng đợi tin chưa đọc
        private readonly Queue<(string from, string text, bool isHostSender)> _pendingLobby = new();
        private readonly Queue<(string from, string text, bool isHostSender)> _pendingRoom = new();
        private int _unreadLobby = 0;
        private int _unreadRoom = 0;

        // Chống double-click Tạo phòng
        private bool _creatingRoom = false;

        // Cờ tránh vòng lặp SelectedIndexChanged → UpdateKenhItems → SelectedIndexChanged...
        private bool _isUpdatingKenhItems = false;

        // ====== HẰNG SỐ KÊNH ======
        private const string KENH_SANH_BASE = "Kênh chung (Sảnh)";
        private const string KENH_PHONG_BASE = "Kênh phòng";

        public Room()
        {
            InitializeComponent();
        }

        private static bool InDesigner()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
            try
            {
                return Process.GetCurrentProcess().ProcessName
                    .IndexOf("devenv", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        private bool DangChonKenhSanh()
            => (cmbKenh.SelectedItem?.ToString()?.StartsWith(KENH_SANH_BASE, StringComparison.OrdinalIgnoreCase) ?? false);

        private bool DangChonKenhPhong()
            => (cmbKenh.SelectedItem?.ToString()?.StartsWith(KENH_PHONG_BASE, StringComparison.OrdinalIgnoreCase) ?? false);

        // Helper chạy an toàn trên UI thread
        private static void SafeOnUI(Control ctl, Action body)
        {
            if (ctl == null || body == null) return;
            if (ctl.IsDisposed) return;

            void Wrapped()
            {
                if (ctl.IsDisposed) return;
                try { body(); }
                catch { }
            }

            if (!ctl.IsHandleCreated)
            {
                void handler(object? s, EventArgs e)
                {
                    try { ctl.HandleCreated -= handler; } catch { }
                    SafeOnUI(ctl, body);
                }

                try { ctl.HandleCreated += handler; } catch { }
                return;
            }

            if (ctl.InvokeRequired)
            {
                try { ctl.BeginInvoke((Action)Wrapped); } catch { }
            }
            else
            {
                Wrapped();
            }
        }

        // ================== Cập nhật combobox kênh + badge ==================
        private void UpdateKenhItems(bool inRoom)
        {
            SafeOnUI(cmbKenh, () =>
            {
                if (cmbKenh == null || cmbKenh.IsDisposed) return;

                _isUpdatingKenhItems = true;
                try
                {
                    // Lưu lại lựa chọn hiện tại để giữ
                    string? current = cmbKenh.SelectedItem as string;

                    // Xây lại string hiển thị
                    string lobbyItem = KENH_SANH_BASE;
                    if (_unreadLobby > 0)
                        lobbyItem += $" (+{_unreadLobby})";

                    string? roomItem = null;
                    if (inRoom && !string.IsNullOrEmpty(currentRoomId))
                    {
                        roomItem = $"{KENH_PHONG_BASE} ({currentRoomId})";
                        if (_unreadRoom > 0)
                            roomItem += $" (+{_unreadRoom})";
                    }

                    cmbKenh.Items.Clear();
                    cmbKenh.Items.Add(lobbyItem);

                    if (roomItem != null)
                        cmbKenh.Items.Add(roomItem);

                    // Ưu tiên giữ đúng kênh mà user đã chọn
                    if (!string.IsNullOrEmpty(current))
                    {
                        if (current.StartsWith(KENH_PHONG_BASE, StringComparison.OrdinalIgnoreCase) && roomItem != null)
                        {
                            cmbKenh.SelectedItem = roomItem;
                            if (cmbKenh.SelectedItem == null && cmbKenh.Items.Count > 1)
                                cmbKenh.SelectedIndex = 1;
                        }
                        else if (current.StartsWith(KENH_SANH_BASE, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbKenh.SelectedItem = lobbyItem;
                            if (cmbKenh.SelectedItem == null && cmbKenh.Items.Count > 0)
                                cmbKenh.SelectedIndex = 0;
                        }
                        else
                        {
                            // Nếu không match base nào thì fallback
                            if (roomItem != null && inRoom && cmbKenh.Items.Count > 1)
                                cmbKenh.SelectedIndex = 1;
                            else
                                cmbKenh.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        // Lần đầu: nếu đang ở phòng thì chọn phòng, không thì chọn sảnh
                        if (roomItem != null && inRoom && cmbKenh.Items.Count > 1)
                            cmbKenh.SelectedIndex = 1;
                        else
                            cmbKenh.SelectedIndex = 0;
                    }
                }
                finally
                {
                    _isUpdatingKenhItems = false;
                }
            });
        }

        private void UI(Action a)
        {
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }

        // =============== LOAD ===============
        private async void Room_Load(object sender, EventArgs e)
        {
            if (InDesigner())
            {
                chatBox.SendToBack();
                cmbKenh.BringToFront();
                txtChat.BringToFront();
                btnSendChat.BringToFront();
                return;
            }

            SetStatus("Chưa tạo/tham gia phòng.");
            UpdateKenhItems(false);

            btnCreateRoom.Enabled = true;
            btnJoinRoom.Enabled = true;
            btnStartGame.Enabled = false;
            btnLeaveRoom.Enabled = false;

            // Chat sảnh LAN
            chatSanh = new ChatSanhLAN();
            chatSanh.BatDauNghe();

            chatSanh.NhanTinSanh += (ten, noiDung) =>
            {
                UI(() =>
                {
                    if (DangChonKenhSanh())
                    {
                        AppendChat($"[LOBBY]{ten}", noiDung, false);
                    }
                    else
                    {
                        _pendingLobby.Enqueue(($"[LOBBY]{ten}", noiDung, false));
                        _unreadLobby++;
                        UpdateKenhItems(_connectedToRoom);
                    }
                });
            };

            // Khi đổi kênh thì xả hàng đợi
            cmbKenh.SelectedIndexChanged += (_, __) =>
            {
                if (_isUpdatingKenhItems) return; // tránh recursion

                if (DangChonKenhSanh())
                {
                    while (_pendingLobby.Count > 0)
                    {
                        var it = _pendingLobby.Dequeue();
                        AppendChat(it.from, it.text, it.isHostSender);
                    }
                    _unreadLobby = 0;
                    UpdateKenhItems(_connectedToRoom);
                }

                if (DangChonKenhPhong())
                {
                    while (_pendingRoom.Count > 0)
                    {
                        var it = _pendingRoom.Dequeue();
                        AppendChat(it.from, it.text, it.isHostSender);
                    }
                    _unreadRoom = 0;
                    UpdateKenhItems(_connectedToRoom);
                }
            };

            await LoadRoomsAsync();
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            shuttingDown = true;

            try { networkManager?.Dispose(); } catch { }
            try { lanBroadcast?.Dispose(); } catch { }
            try { chatSanh?.Dispose(); } catch { }

            try
            {
                if (isHost && !string.IsNullOrEmpty(currentRoomId))
                {
                    if (gameStarted) _ = RoomApi.EndRoomAsync(currentRoomId);
                    else _ = RoomApi.CancelRoomAsync(currentRoomId);
                }
            }
            catch { }
        }

        private void SetStatus(string msg)
        {
            if (InvokeRequired) Invoke(new Action(() => lblStatus.Text = msg));
            else lblStatus.Text = msg;
        }

        private static string MakeRoomId()
            => new Random().Next(100000, 999999).ToString();

        // =============== HIỂN THỊ CHAT ===============
        private void AppendChat(string from, string text, bool isHostSender)
        {
            if (chatBox == null) return;

            if (chatBox.InvokeRequired)
            {
                chatBox.Invoke(new Action(() => AppendChat(from, text, isHostSender)));
                return;
            }

            bool laLobby = from.StartsWith("[LOBBY]");
            if (laLobby) from = from.Substring("[LOBBY]".Length);

            string prefix = laLobby ? "[LOBBY] " : (isHostSender ? "[HOST] " : "[CLIENT] ");
            Color prefixColor = laLobby ? Color.Cyan : (isHostSender ? Color.Red : Color.LightGray);
            Color nameColor = Color.Gold;
            Color textColor = Color.White;

            if (chatBox.TextLength > 0) chatBox.AppendText(Environment.NewLine);

            int start = chatBox.TextLength;

            chatBox.AppendText(prefix);
            chatBox.Select(start, prefix.Length);
            chatBox.SelectionColor = prefixColor;

            string namePart = from + ": ";
            chatBox.AppendText(namePart);
            chatBox.Select(start + prefix.Length, namePart.Length);
            chatBox.SelectionColor = nameColor;

            chatBox.AppendText(text);
            chatBox.SelectionColor = textColor;

            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.ScrollToCaret();
        }

        // =============== HOST LISTEN ===============
        private void StartHostServer()
        {
            try { networkManager?.Dispose(); } catch { }
            networkManager = new NetworkManager();
            WireNetworkEvents();
            networkManager.StartHost(GAME_PORT);
        }

        private async Task HandlePeerDisconnectedAsync()
        {
            if (!isHost || gameStarted || string.IsNullOrEmpty(currentRoomId) || shuttingDown) return;

            try
            {
                StartHostServer();
                var ok = await RoomApi.BackToCreatedAsync(currentRoomId);

                if (ok)
                {
                    UI(() =>
                    {
                        btnStartGame.Enabled = false;
                        SetStatus($"Người chơi rời phòng. Phòng {currentRoomId} trở lại trạng thái chờ (1/2).");
                        btnLeaveRoom.Enabled = true;
                    });
                    UpdateKenhItems(true);
                    await LoadRoomsAsync();
                }
                else
                {
                    UI(() =>
                    {
                        btnStartGame.Enabled = false;
                        SetStatus($"Người chơi rời phòng, nhưng API BackToCreatedAsync thất bại (room {currentRoomId}).");
                        btnLeaveRoom.Enabled = true;
                    });
                }
            }
            catch (Exception ex)
            {
                UI(() =>
                {
                    btnStartGame.Enabled = false;
                    SetStatus("Lỗi khi cập nhật phòng về CREATED: " + ex.Message);
                    btnLeaveRoom.Enabled = true;
                });
            }
        }

        private void WireNetworkEvents()
        {
            if (networkManager == null) return;

            networkManager.OnPeerConnected += () =>
            {
                _connectedToRoom = true;

                UI(() =>
                {
                    UpdateKenhItems(true);

                    if (isHost)
                    {
                        btnStartGame.Enabled = true;
                        btnLeaveRoom.Enabled = true;
                        SetStatus($"Phòng {currentRoomId}: ĐÃ ĐỦ 2 NGƯỜI. Host có thể bấm BẮT ĐẦU.");
                    }
                    else
                    {
                        if (cmbKenh.Items.Count > 1)
                            cmbKenh.SelectedIndex = 1;

                        btnLeaveRoom.Enabled = true;
                        SetStatus($"Đã kết nối tới phòng {currentRoomId}. Chờ host bấm BẮT ĐẦU.");
                    }
                });
            };

            networkManager.OnMessageReceived += (msg) =>
            {
                if (msg.StartsWith("LEFT_ROOM|"))
                {
                    string who = msg.Substring("LEFT_ROOM|".Length);
                    UI(() =>
                    {
                        var line = ("Hệ thống", $"{who} đã rời phòng.", true);
                        if (DangChonKenhPhong()) AppendChat(line.Item1, line.Item2, line.Item3);
                        else
                        {
                            _pendingRoom.Enqueue(line);
                            _unreadRoom++;
                            UpdateKenhItems(true);
                        }
                    });
                    return;
                }

                if (msg == "START_GAME")
                {
                    UI(() => { gameStarted = true; OpenGame(); });
                    return;
                }

                if (msg.StartsWith("CHAT|"))
                {
                    try
                    {
                        var parts = msg.Split(new[] { '|' }, 4);
                        if (parts.Length >= 4)
                        {
                            string role = parts[1];
                            string from = parts[2];
                            string text = parts[3];
                            bool isHostSender = role.Equals("HOST", StringComparison.OrdinalIgnoreCase);

                            UI(() =>
                            {
                                var line = (from, text, isHostSender);

                                if (DangChonKenhPhong())
                                    AppendChat(line.from, line.text, line.isHostSender);
                                else
                                {
                                    _pendingRoom.Enqueue(line);
                                    _unreadRoom++;
                                    UpdateKenhItems(true);
                                }
                            });
                        }
                    }
                    catch { }
                    return;
                }

                if (msg == "ROOM_NOT_FOUND")
                {
                    UI(() =>
                    {
                        SetStatus("Không tìm thấy phòng trên server.");
                        btnJoinRoom.Enabled = true;
                        btnCreateRoom.Enabled = true;
                        btnLeaveRoom.Enabled = false;
                    });
                }
            };

            networkManager.OnDisconnected += () =>
            {
                _connectedToRoom = false;

                UI(() => { btnStartGame.Enabled = false; });

                if (shuttingDown) return;

                if (isHost && !gameStarted)
                {
                    _ = HandlePeerDisconnectedAsync();
                }
                else if (!isHost)
                {
                    UI(() =>
                    {
                        SetStatus("Host đã rời phòng. Bạn đã bị ngắt kết nối.");
                        btnJoinRoom.Enabled = true;
                        btnCreateRoom.Enabled = true;
                        btnLeaveRoom.Enabled = false;
                        UpdateKenhItems(false);
                        currentRoomId = null;
                        gameStarted = false;
                    });
                }
            };
        }

        // =============== ROOMS LIST ===============
        private async Task LoadRoomsAsync()
        {
            try
            {
                var rooms = await RoomApi.GetRoomsAsync();

                UI(() =>
                {
                    IdRoom.Rows.Clear();
                    if (rooms == null || rooms.Count == 0) return;

                    foreach (RoomApi.RoomInfo r in rooms)
                    {
                        string playersPart = $"{r.PlayerCount}/2";
                        string statusText = string.IsNullOrWhiteSpace(r.Status)
                            ? playersPart
                            : $"{playersPart} - {r.Status}";
                        IdRoom.Rows.Add(r.RoomId, r.Host, statusText);
                    }
                });
            }
            catch (Exception ex)
            {
                SetStatus("Không tải được danh sách phòng: " + ex.Message);
            }
        }

        // =============== TẠO PHÒNG ===============
        private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (_creatingRoom) return;
            _creatingRoom = true;

            try
            {
                if (!NetworkManager.IsNetworkAvailable())
                {
                    SetStatus("Không phát hiện mạng LAN khả dụng!");
                    return;
                }

                currentRoomId = string.IsNullOrWhiteSpace(txtRoomID.Text)
                    ? MakeRoomId()
                    : txtRoomID.Text.Trim();
                txtRoomID.Text = currentRoomId;

                try { networkManager?.Dispose(); } catch { }
                try { lanBroadcast?.Dispose(); } catch { }

                gameStarted = false;
                shuttingDown = false;

                isHost = true;
                btnStartGame.Enabled = false;
                _connectedToRoom = false;

                btnJoinRoom.Enabled = false;
                btnCreateRoom.Enabled = false;
                btnLeaveRoom.Enabled = true;

                lanBroadcast = new LANBroadcast();
                lanBroadcast.StartBroadcast(currentRoomId, GAME_PORT);

                StartHostServer();

                SetStatus($"[HOST] Đã tạo phòng {currentRoomId}. Đang chờ người chơi khác...");
                UpdateKenhItems(true);

                // Host tự động chuyển sang Kênh phòng sau khi tạo
                SafeOnUI(cmbKenh, () =>
                {
                    if (cmbKenh.Items.Count > 1)
                        cmbKenh.SelectedIndex = 1;
                });

                var hostName = string.IsNullOrWhiteSpace(AccountData.Username)
                    ? "Host"
                    : AccountData.Username;
                _ = RoomLogger.LogHost(currentRoomId, hostName);

                var ok = await RoomApi.CreateRoomAsync(currentRoomId, hostName);
                if (!ok)
                {
                    SetStatus($"[HOST] Đã tạo phòng {currentRoomId} (LAN OK) nhưng POST API thất bại.");
                }

                await LoadRoomsAsync();
            }
            catch (Exception ex)
            {
                SetStatus("Lỗi tạo phòng (LAN): " + ex.Message);
                btnCreateRoom.Enabled = true;
                btnJoinRoom.Enabled = true;
                btnLeaveRoom.Enabled = false;
            }
            finally
            {
                _creatingRoom = false;
            }
        }

        // =============== JOIN PHÒNG ===============
        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            string roomId = txtRoomID.Text.Trim();

            if (string.IsNullOrEmpty(roomId))
            {
                SetStatus("Nhập Room ID trước!");
                return;
            }

            if (!NetworkManager.IsNetworkAvailable())
            {
                SetStatus("Không phát hiện mạng LAN khả dụng!");
                return;
            }

            currentRoomId = roomId;
            isHost = false;
            gameStarted = false;
            shuttingDown = false;
            _connectedToRoom = false;

            btnJoinRoom.Enabled = false;
            btnStartGame.Enabled = false;
            btnCreateRoom.Enabled = false;
            btnLeaveRoom.Enabled = true;

            try { networkManager?.Dispose(); } catch { }
            try { lanBroadcast?.Dispose(); } catch { }

            lanBroadcast = new LANBroadcast();
            SetStatus($"[CLIENT] Đang tìm phòng {roomId} trong LAN...");

            lanBroadcast.OnRoomFound += async (foundRoomId, hostIp, port) =>
            {
                if (foundRoomId != currentRoomId) return;

                try
                {
                    lanBroadcast.StopListen();

                    UI(() => SetStatus($"Tìm thấy host {hostIp}:{port}, đang kết nối..."));

                    networkManager = new NetworkManager();
                    WireNetworkEvents();

                    await networkManager.JoinHost(hostIp, port);

                    UI(() =>
                    {
                        SetStatus($"Đã kết nối tới host {hostIp}. Chờ host bấm BẮT ĐẦU.");
                        UpdateKenhItems(true);
                        if (cmbKenh.Items.Count > 1) cmbKenh.SelectedIndex = 1;
                        btnLeaveRoom.Enabled = true;
                    });

                    var guestName = string.IsNullOrWhiteSpace(AccountData.Username)
                        ? "Client"
                        : AccountData.Username;
                    _ = RoomLogger.LogGuest(currentRoomId, guestName);

                    var ok = await RoomApi.JoinRoomAsync(currentRoomId, guestName);
                    if (!ok)
                    {
                        UI(() => SetStatus($"Đã join LAN nhưng POST API join thất bại (phòng {currentRoomId})."));
                    }

                    await LoadRoomsAsync();
                }
                catch (Exception ex)
                {
                    UI(() =>
                    {
                        SetStatus("Lỗi kết nối host: " + ex.Message);
                        btnJoinRoom.Enabled = true;
                        btnCreateRoom.Enabled = true;
                        btnLeaveRoom.Enabled = false;
                        UpdateKenhItems(false);
                    });
                }
            };

            lanBroadcast.StartListen(currentRoomId);
        }

        // =============== START GAME ===============
        private async void btnStartGame_Click(object sender, EventArgs e)
        {
            if (networkManager == null || !networkManager.IsConnected)
            {
                SetStatus("Chưa đủ 2 người để bắt đầu!");
                return;
            }

            gameStarted = true;

            if (isHost)
            {
                var hostName = string.IsNullOrWhiteSpace(AccountData.Username)
                    ? "Host"
                    : AccountData.Username;
                _ = RoomApi.StartRoomAsync(currentRoomId, hostName);
            }

            networkManager.Send("START_GAME");
            OpenGame();
        }

        // =============== THOÁT PHÒNG ===============
        private async void btnLeaveRoom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRoomId))
            {
                SetStatus("Hiện không ở trong phòng nào để thoát.");
                btnLeaveRoom.Enabled = false;
                return;
            }

            // Client báo host trước khi rời
            if (!isHost && networkManager != null && networkManager.IsConnected)
            {
                string who = string.IsNullOrWhiteSpace(AccountData.Username)
                    ? "Client"
                    : AccountData.Username;
                try { networkManager.Send("LEFT_ROOM|" + who); } catch { }
            }

            shuttingDown = true;

            try { networkManager?.Dispose(); } catch { }
            try { lanBroadcast?.Dispose(); } catch { }

            if (isHost)
            {
                try
                {
                    if (gameStarted) _ = RoomApi.EndRoomAsync(currentRoomId);
                    else _ = RoomApi.CancelRoomAsync(currentRoomId);
                    SetStatus($"Bạn đã thoát và đóng phòng {currentRoomId}.");
                }
                catch (Exception ex)
                {
                    SetStatus("Lỗi khi hủy/đóng phòng: " + ex.Message);
                }
            }
            else
            {
                SetStatus($"Bạn đã rời phòng {currentRoomId}.");
            }

            isHost = false;
            gameStarted = false;
            currentRoomId = null;
            shuttingDown = false;
            _connectedToRoom = false;

            _pendingLobby.Clear();
            _pendingRoom.Clear();
            _unreadLobby = _unreadRoom = 0;

            UpdateKenhItems(false);

            btnCreateRoom.Enabled = true;
            btnJoinRoom.Enabled = true;
            btnStartGame.Enabled = false;
            btnLeaveRoom.Enabled = false;

            try { await LoadRoomsAsync(); } catch { }
        }

        // =============== GAME FORM ===============
        private void OpenGame()
        {
            var game = new GAMESOLO(networkManager, isHost, currentRoomId);
            game.Show();

            game.FormClosed += async (_, __) =>
            {
                SetStatus("Đã quay lại lobby.");
                UpdateKenhItems(false);

                btnCreateRoom.Enabled = true;
                btnJoinRoom.Enabled = true;
                btnStartGame.Enabled = false;
                btnLeaveRoom.Enabled = false;

                currentRoomId = null;
                isHost = false;
                gameStarted = false;
                _connectedToRoom = false;

                await LoadRoomsAsync();
            };
        }

        // =============== UI MISC ===============
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Vui lòng đăng nhập để xem lịch sử.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var form = new MatchHistoryForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở lịch sử đấu: " + ex.Message);
            }
        }

        private void IdRoom_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void IdRoom_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = IdRoom.Rows[e.RowIndex];
            var roomIdObj = row.Cells["Player1"]?.Value;
            var roomId = roomIdObj?.ToString();
            if (string.IsNullOrWhiteSpace(roomId)) return;

            txtRoomID.Text = roomId;
            btnJoinRoom_Click(btnJoinRoom, EventArgs.Empty);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try { await LoadRoomsAsync(); }
            catch (Exception ex) { SetStatus("Không tải được danh sách phòng: " + ex.Message); }
        }

        // =============== GỬI CHAT ===============
        private async void btnSendChat_Click(object sender, EventArgs e)
        {
            string text = txtChat.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string from = string.IsNullOrWhiteSpace(AccountData.Username)
                ? (isHost ? "Host" : "Client")
                : AccountData.Username;

            bool inRoom = !string.IsNullOrEmpty(currentRoomId);

            bool guiSanh = DangChonKenhSanh();
            bool guiPhong = DangChonKenhPhong();

            // Chưa chọn kênh
            if (!guiSanh && !guiPhong)
            {
                MessageBox.Show("Hãy chọn kênh chat (Sảnh hoặc Kênh phòng).");
                return;
            }

            // Chọn kênh phòng nhưng chưa ở trong phòng
            if (guiPhong && !inRoom)
            {
                MessageBox.Show("Bạn chưa tham gia phòng nào, không thể chat kênh phòng.");
                return;
            }

            // ========== KÊNH SẢNH (UDP broadcast) ==========
            if (guiSanh)
            {
                if (chatSanh == null)
                {
                    chatSanh = new ChatSanhLAN();
                    chatSanh.BatDauNghe();
                    chatSanh.NhanTinSanh += (ten, noiDung) =>
                    {
                        UI(() =>
                        {
                            if (DangChonKenhSanh())
                            {
                                AppendChat($"[LOBBY]{ten}", noiDung, false);
                            }
                            else
                            {
                                _pendingLobby.Enqueue(($"[LOBBY]{ten}", noiDung, false));
                                _unreadLobby++;
                                UpdateKenhItems(_connectedToRoom);
                            }
                        });
                    };
                }

                try { await chatSanh.GuiTinSanhAsync(from, text); } catch { }

                if (DangChonKenhSanh())
                {
                    AppendChat($"[LOBBY]{from}", text, false);
                }
                else
                {
                    _pendingLobby.Enqueue(($"[LOBBY]{from}", text, false));
                    _unreadLobby++;
                    UpdateKenhItems(_connectedToRoom);
                }

                txtChat.Clear();
                return;
            }

            // ========== KÊNH PHÒNG (TCP) ==========
            if (guiPhong && inRoom)
            {
                string role = isHost ? "HOST" : "CLIENT";
                string payload = $"CHAT|{role}|{from}|{text}";

                if (DangChonKenhPhong())
                {
                    AppendChat(from, text, isHost);
                }
                else
                {
                    _pendingRoom.Enqueue((from, text, isHost));
                    _unreadRoom++;
                    UpdateKenhItems(true);
                }

                txtChat.Clear();

                try { networkManager.Send(payload); } catch { }
            }
        }

        private void chatBox_TextChanged(object sender, EventArgs e) { }
        private void txtChat_TextChanged(object sender, EventArgs e) { }
    }
}
