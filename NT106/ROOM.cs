using System;
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

        private const int GAME_PORT = 9000; // TCP game port

        private bool gameStarted = false;
        private bool shuttingDown = false;

        public Room()
        {
            InitializeComponent();
        }

        private async void Room_Load(object sender, EventArgs e)
        {
            SetStatus("Chưa tạo/tham gia phòng.");

            // Bật chat sảnh LAN
            chatSanh = new ChatSanhLAN();
            chatSanh.BatDauNghe();
            chatSanh.NhanTinSanh += (ten, noiDung) =>
                UI(() => AppendChat($"[LOBBY]{ten}", noiDung, false));

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
                    if (gameStarted)
                        _ = RoomApi.EndRoomAsync(currentRoomId);
                    else
                        _ = RoomApi.CancelRoomAsync(currentRoomId);
                }
            }
            catch { }
        }

        private void SetStatus(string msg)
        {
            if (InvokeRequired) Invoke(new Action(() => lblStatus.Text = msg));
            else lblStatus.Text = msg;
        }

        private static string MakeRoomId() => new Random().Next(100000, 999999).ToString();

        private void UI(Action a) { if (InvokeRequired) BeginInvoke(a); else a(); }

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
                    });
                    await LoadRoomsAsync();
                }
                else
                {
                    UI(() =>
                    {
                        btnStartGame.Enabled = false;
                        SetStatus($"Người chơi rời phòng, nhưng API BackToCreatedAsync thất bại (room {currentRoomId}).");
                    });
                }
            }
            catch (Exception ex)
            {
                UI(() =>
                {
                    btnStartGame.Enabled = false;
                    SetStatus("Lỗi khi cập nhật phòng về CREATED: " + ex.Message);
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
                        SetStatus($"Phòng {currentRoomId}: ĐÃ ĐỦ 2 NGƯỜI. Host có thể bấm BẮT ĐẦU.");
                    }
                    else
                    {
                        SetStatus($"Đã kết nối tới phòng {currentRoomId}. Chờ host bấm BẮT ĐẦU.");
                    }
                });
            };

            networkManager.OnMessageReceived += (msg) =>
            {
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
                    });
                    return;
                }
            };

            networkManager.OnDisconnected += () =>
            {
                UI(() =>
                {
                    btnStartGame.Enabled = false;
                    SetStatus("Kết nối bị ngắt. Quay lại lobby hoặc tạo phòng khác.");
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
                    });
                }
            };
        }

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

                lanBroadcast = new LANBroadcast();
                lanBroadcast.StartBroadcast(currentRoomId, GAME_PORT);

                StartHostServer();

                SetStatus($"[HOST] Đã tạo phòng {currentRoomId}. Đang chờ người chơi khác...");

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
            }
        }

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

                    UI(() => SetStatus($"Đã kết nối tới host {hostIp}. Chờ host bấm BẮT ĐẦU."));

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
                    });
                }
            };

            lanBroadcast.StartListen(currentRoomId);
        }

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

        private void OpenGame()
        {
            var game = new GAMESOLO(networkManager, isHost, currentRoomId);
            game.Show();
            this.Hide();

            game.FormClosed += async (_, __) =>
            {
                this.Show();
                SetStatus("Đã quay lại lobby.");
                await LoadRoomsAsync();
            };
        }

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

            // Đổi "Player1" thành đúng tên cột RoomId trong Designer của bạn nếu khác
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

        private async void btnSendChat_Click(object sender, EventArgs e)
        {
            string text = txtChat.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string from = string.IsNullOrWhiteSpace(AccountData.Username)
                ? (isHost ? "Host" : "Client")
                : AccountData.Username;

            bool daTrongPhong = networkManager != null && networkManager.IsConnected && !string.IsNullOrEmpty(currentRoomId);

            if (!daTrongPhong)
            {
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
