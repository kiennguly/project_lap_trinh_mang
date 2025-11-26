using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    /// <summary>
    /// Quản lý kết nối LAN thuần bằng TCP (không dùng WebSocket/AWS).
    /// - Host: lắng nghe client.
    /// - Client: kết nối tới Host.
    /// - Gửi/nhận: dữ liệu game, chat trong phòng, tín hiệu bắt đầu game.
    /// </summary>
    public class QuanLyMangLAN : IDisposable
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public bool LaHost { get; private set; }
        public bool DaKetNoi => _client != null && _client.Connected && _stream != null;

        // Sự kiện cho Room/GAME form đăng ký
        public event Action<string> NhanThongDiepGame;           // payload game
        public event Action<string, string> NhanTinChat;         // (tenHienThi, noiDung)
        public event Action BatDauGame;                          // khi nhận lệnh bắt đầu
        public event Action MatKetNoi;                           // khi bên kia ngắt kết nối

        private const string TYPE_GAME = "GAME";
        private const string TYPE_CHAT = "CHAT";
        private const string TYPE_CTRL = "CTRL";

        // ==================== HOST ====================
        /// <summary>
        /// Bắt đầu làm Host (server TCP) trên port chỉ định.
        /// Hàm này chờ tới khi có 1 client kết nối thì mới return.
        /// </summary>
        public async Task BatDauHostAsync(int port)
        {
            LaHost = true;
            _cts = new CancellationTokenSource();

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            try
            {
                _client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                _stream = _client.GetStream();

                _ = Task.Run(() => VongNhanDuLieuAsync(_cts.Token));
            }
            catch
            {
                HuyKetNoi();
                throw;
            }
        }

        // ==================== CLIENT ====================
        /// <summary>
        /// Client kết nối tới Host LAN bằng IP và port.
        /// </summary>
        public async Task KetNoiDenHostAsync(string ipHost, int port)
        {
            LaHost = false;
            _cts = new CancellationTokenSource();

            _client = new TcpClient();
            try
            {
                await _client.ConnectAsync(IPAddress.Parse(ipHost), port).ConfigureAwait(false);
                _stream = _client.GetStream();

                _ = Task.Run(() => VongNhanDuLieuAsync(_cts.Token));
            }
            catch
            {
                HuyKetNoi();
                throw;
            }
        }

        // ==================== API GỬI TIN ====================
        /// <summary>
        /// Gửi dữ liệu game (payload tự định nghĩa, ví dụ JSON).
        /// </summary>
        public Task GuiThongDiepGameAsync(string payload)
        {
            return GuiDongAsync($"{TYPE_GAME}|{payload}");
        }

        /// <summary>
        /// Gửi tin nhắn chat trong phòng.
        /// Ví dụ tenHienThi = [123456][HOST][kien]
        /// </summary>
        public Task GuiTinChatAsync(string tenHienThi, string noiDung)
        {
            string dong = $"{TYPE_CHAT}|{tenHienThi}|{noiDung}";
            return GuiDongAsync(dong);
        }

        /// <summary>
        /// Gửi lệnh bắt đầu game (host gọi).
        /// </summary>
        public Task GuiLenhBatDauGameAsync()
        {
            return GuiDongAsync($"{TYPE_CTRL}|START_GAME");
        }

        private async Task GuiDongAsync(string dong)
        {
            if (!DaKetNoi) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(dong + "\n");
                await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            catch
            {
                // Nếu lỗi ghi -> coi như bị rớt kết nối
                MatKetNoi?.Invoke();
                HuyKetNoi();
            }
        }

        // ==================== VÒNG NHẬN DỮ LIỆU ====================
        private async Task VongNhanDuLieuAsync(CancellationToken token)
        {
            var reader = new StreamReader(_stream, Encoding.UTF8);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    string dong = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (dong == null)
                    {
                        // bên kia đóng kết nối
                        break;
                    }

                    XuLyDongNhanDuoc(dong);
                }
            }
            catch
            {
                // bỏ qua, sẽ báo mất kết nối bên dưới
            }

            MatKetNoi?.Invoke();
            HuyKetNoi();
        }

        private void XuLyDongNhanDuoc(string dong)
        {
            // Format:
            // GAME|payload
            // CHAT|tenHienThi|noiDung
            // CTRL|START_GAME
            var parts = dong.Split('|');
            if (parts.Length == 0) return;

            string loai = parts[0];

            switch (loai)
            {
                case TYPE_GAME:
                    if (parts.Length >= 2)
                    {
                        // Giữ nguyên phần payload sau "GAME|"
                        string payload = dong.Substring(TYPE_GAME.Length + 1);
                        NhanThongDiepGame?.Invoke(payload);
                    }
                    break;

                case TYPE_CHAT:
                    if (parts.Length >= 3)
                    {
                        string tenHienThi = parts[1];
                        // CHAT|ten|noiDung -> cắt phần sau CHAT|ten|
                        string prefix = $"{TYPE_CHAT}|{tenHienThi}|";
                        string noiDung = dong.Length > prefix.Length
                            ? dong.Substring(prefix.Length)
                            : string.Empty;

                        NhanTinChat?.Invoke(tenHienThi, noiDung);
                    }
                    break;

                case TYPE_CTRL:
                    if (parts.Length >= 2 && parts[1] == "START_GAME")
                    {
                        BatDauGame?.Invoke();
                    }
                    break;
            }
        }

        // ==================== HỦY / DISPOSE ====================
        public void HuyKetNoi()
        {
            try { _cts?.Cancel(); } catch { }
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            try { _listener?.Stop(); } catch { }

            _stream = null;
            _client = null;
            _listener = null;
            _cts = null;
        }

        public void Dispose()
        {
            HuyKetNoi();
        }
    }
}
