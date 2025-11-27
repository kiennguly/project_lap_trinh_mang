using System;
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

        // Chat sảnh LAN (broadcast UDP)
        private ChatSanhLAN chatSanh;

        private bool isHost;
        private string currentRoomId;

        private const int GAME_PORT = 9000;

        private bool gameStarted = false;
        private bool shuttingDown = false;

        public Room()
        {
            InitializeComponent(); 
        }

        // === Helper: xác định đang ở WinForms Designer ===
        private static bool InDesigner()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
            try { return Process.GetCurrentProcess().ProcessName.IndexOf("devenv", StringComparison.OrdinalIgnoreCase) >= 0; }
            catch { return false; }
        }

        // === Helper: cập nhật danh sách kênh chat ===
        private void UpdateKenhItems(bool inRoom)
        {
            cmbKenh.Items.Clear();
            cmbKenh.Items.Add("Kênh chung (Sảnh)");
            if (inRoom && !string.IsNullOrEmpty(currentRoomId))
                cmbKenh.Items.Add($"Kênh phòng ({currentRoomId})");
            cmbKenh.SelectedIndex = 0;
        }

        // ========================= FORM LOAD =========================
        private async void Room_Load(object sender, EventArgs e)
        {
            // Nếu đang mở ở Designer: đừng chạy socket/API, chỉ sắp z-order để dễ kéo thả
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

            // Trạng thái nút ban đầu
            btnCreateRoom.Enabled = true;
            btnJoinRoom.Enabled = true;
            btnStartGame.Enabled = false;
            btnLeaveRoom.Enabled = false;

            // Bật chat sảnh LAN
            chatSanh = new ChatSanhLAN();
            chatSanh.BatDauNghe();
            chatSanh.NhanTinSanh += (ten, noiDung) =>
                UI(() => AppendChat($"[LOBBY]{ten}", noiDung, false));

            await LoadRoomsAsync();
        }

        // ===== FORM CLOSING =====
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
                    if (gameStarted)
                        _ = RoomApi.EndRoomAsync(currentRoomId);
                    else
                        _ = RoomApi.CancelRoomAsync(currentRoomId);
                }
            }
            catch { }
        }

        // ========================= HELPER =========================
        private void SetStatus(string msg)
        {
            if (InvokeRequired) Invoke(new Action(() => lblStatus.Text = msg));
            else lblStatus.Text = msg;
        }

        private static string MakeRoomId() => new Random().Next(100000, 999999).ToString();

        private void UI(Action a) { if (InvokeRequired) BeginInvoke(a); else a(); }

        // ========================= CHAT RICH TEXT =========================
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

        // ====== START HOST TCP ======
        private void StartHostServer()
        {
            try { networkManager?.Dispose(); } catch { }

            networkManager = new NetworkManager();
            WireNetworkEvents();
            networkManager.StartHost(GAME_PORT);
        }

        // Host xử lý khi người chơi còn lại rời phòng trước khi bắt đầu
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
                UI(() =>
                {
                    if (isHost)
                    {
                        btnStartGame.Enabled = true;
                        btnLeaveRoom.Enabled = true;
                        SetStatus($"Phòng {currentRoomId}: ĐÃ ĐỦ 2 NGƯỜI. Host có thể bấm BẮT ĐẦU.");
                    }
                    else
                    {
                        btnLeaveRoom.Enabled = true;
                        SetStatus($"Đã kết nối tới phòng {currentRoomId}. Chờ host bấm BẮT ĐẦU.");
                    }
                });
            };

            networkManager.OnMessageReceived += (msg) =>
            {
                // THÔNG BÁO CLIENT RỜI PHÒNG
                if (msg.StartsWith("LEFT_ROOM|"))
                {
                    string who = msg.Substring("LEFT_ROOM|".Length);
                    // hiện trong chat như một thông báo hệ thống phía host
                    AppendChat("Hệ thống", $"{who} đã rời phòng.", true);
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
                            AppendChat(from, text, isHostSender);
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
                    return;
                }
            };

            networkManager.OnDisconnected += () =>
            {
                UI(() =>
                {
                    btnStartGame.Enabled = false;
                });

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

        //       LOAD DANH SÁCH PHÒNG
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
                        string statusText = string.IsNullOrWhiteSpace(r.Status) ? playersPart : $"{playersPart} - {r.Status}";
                        IdRoom.Rows.Add(r.RoomId, r.Host, statusText);
                    }
                });
            }
            catch (Exception ex)
            {
                SetStatus("Không tải được danh sách phòng: " + ex.Message);
            }
        }

        // ========================= TẠO PHÒNG =========================
        private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            try
            {
                if (!NetworkManager.IsNetworkAvailable())
                {
                    SetStatus("Không phát hiện mạng LAN khả dụng!");
                    return;
                }

                currentRoomId = string.IsNullOrWhiteSpace(txtRoomID.Text) ? MakeRoomId() : txtRoomID.Text.Trim();
                txtRoomID.Text = currentRoomId;

                try { networkManager?.Dispose(); } catch { }
                try { lanBroadcast?.Dispose(); } catch { }

                gameStarted = false;
                shuttingDown = false;

                isHost = true;
                btnStartGame.Enabled = false;

                // Sau khi tạo phòng: khóa luôn Join + Create, chỉ cho Thoát
                btnJoinRoom.Enabled = false;    // không cho tự join phòng nữa
                btnCreateRoom.Enabled = false;  // tránh spam tạo phòng
                btnLeaveRoom.Enabled = true;

                // Broadcast LAN
                lanBroadcast = new LANBroadcast();
                lanBroadcast.StartBroadcast(currentRoomId, GAME_PORT);

                // Start TCP Host
                StartHostServer();

                SetStatus($"[HOST] Đã tạo phòng {currentRoomId}. Đang chờ người chơi khác...");
                UpdateKenhItems(true);

                // Log + API
                var hostName = string.IsNullOrWhiteSpace(AccountData.Username) ? "Host" : AccountData.Username;
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
        }

        // ========================= JOIN PHÒNG =========================
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
            btnJoinRoom.Enabled = false;
            btnStartGame.Enabled = false;

            // client đang join: tạm thời không cho tạo phòng khác
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
                        btnLeaveRoom.Enabled = true;
                    });

                    var guestName = string.IsNullOrWhiteSpace(AccountData.Username) ? "Client" : AccountData.Username;
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

        // ========================= HOST BẤM START =========================
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
                var hostName = string.IsNullOrWhiteSpace(AccountData.Username) ? "Host" : AccountData.Username;
                _ = RoomApi.StartRoomAsync(currentRoomId, hostName);
            }

            networkManager.Send("START_GAME");
            OpenGame();
        }

        // ========================= NÚT THOÁT PHÒNG =========================
        private async void btnLeaveRoom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRoomId))
            {
                SetStatus("Hiện không ở trong phòng nào để thoát.");
                btnLeaveRoom.Enabled = false;
                return;
            }

            // Nếu là CLIENT: gửi thông báo cho host trước khi đóng kết nối
            if (!isHost && networkManager != null && networkManager.IsConnected)
            {
                string who = string.IsNullOrWhiteSpace(AccountData.Username)
                    ? "Client"
                    : AccountData.Username;

                try
                {
                    networkManager.Send("LEFT_ROOM|" + who);
                }
                catch
                {
                    // bỏ qua, vẫn đóng kết nối bình thường
                }
            }

            shuttingDown = true;

            try { networkManager?.Dispose(); } catch { }
            try { lanBroadcast?.Dispose(); } catch { }

            if (isHost)
            {
                try
                {
                    if (gameStarted)
                        _ = RoomApi.EndRoomAsync(currentRoomId);
                    else
                        _ = RoomApi.CancelRoomAsync(currentRoomId);

                    SetStatus($"Bạn đã thoát và đóng phòng {currentRoomId}.");
                }
                catch (Exception ex)
                {
                    SetStatus("Lỗi khi hủy/đóng phòng: " + ex.Message);
                }
            }
            else
            {
                // Client rời phòng
                SetStatus($"Bạn đã rời phòng {currentRoomId}.");
                // Nếu có API LeaveRoom thì gọi ở đây
                // _ = RoomApi.LeaveRoomAsync(currentRoomId, AccountData.Username);
            }

            isHost = false;
            gameStarted = false;
            currentRoomId = null;
            shuttingDown = false;

            UpdateKenhItems(false);

            btnCreateRoom.Enabled = true;
            btnJoinRoom.Enabled = true;
            btnStartGame.Enabled = false;
            btnLeaveRoom.Enabled = false;

            try { await LoadRoomsAsync(); } catch { }
        }

        // ========================= MỞ GAME =========================
        private void OpenGame()
        {
            var game = new GAMESOLO(networkManager, isHost, currentRoomId);
            game.Show();
            this.Hide();

            game.FormClosed += async (_, __) =>
            {
                this.Show();
                SetStatus("Đã quay lại lobby.");
                UpdateKenhItems(false);

                // Khi quay lại lobby, reset nút về trạng thái bình thường
                btnCreateRoom.Enabled = true;
                btnJoinRoom.Enabled = true;
                btnStartGame.Enabled = false;
                btnLeaveRoom.Enabled = false;

                currentRoomId = null;
                isHost = false;
                gameStarted = false;

                await LoadRoomsAsync();
            };
        }

        // ========================= LỊCH SỬ ĐẤU =========================
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

        // ========================= GỬI CHAT =========================
        private async void btnSendChat_Click(object sender, EventArgs e)
        {
            string text = txtChat.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string from = string.IsNullOrWhiteSpace(AccountData.Username)
                ? (isHost ? "Host" : "Client")
                : AccountData.Username;

            bool inRoom = networkManager != null && networkManager.IsConnected && !string.IsNullOrEmpty(currentRoomId);
            bool guiKenhPhong = cmbKenh.SelectedItem?.ToString()?.StartsWith("Kênh phòng") == true;

            if (!inRoom || !guiKenhPhong)
            {
                // Kênh sảnh
                if (chatSanh == null)
                {
                    chatSanh = new ChatSanhLAN();
                    chatSanh.BatDauNghe();
                    chatSanh.NhanTinSanh += (ten, noiDung) =>
                        UI(() => AppendChat($"[LOBBY]{ten}", noiDung, false));
                }

                try { await chatSanh.GuiTinSanhAsync(from, text); } catch { }
                AppendChat($"[LOBBY]{from}", text, false);
                txtChat.Clear();
                return;
            }

            // Kênh phòng
            string role = isHost ? "HOST" : "CLIENT";
            string payload = $"CHAT|{role}|{from}|{text}";

            AppendChat(from, text, isHost);
            txtChat.Clear();

            try { networkManager.Send(payload); } catch { }
        }

        private void chatBox_TextChanged(object sender, EventArgs e) { }
        private void txtChat_TextChanged(object sender, EventArgs e) { }
    }
}
