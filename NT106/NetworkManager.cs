using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    /// <summary>
    /// NetworkManager phiên bản LAN:
    ///  - Host: StartHost(port)
    ///  - Client: JoinHost(hostIp, port)
    ///  - Send(string msg): gửi chuỗi, mỗi message kết thúc bằng '\n'
    ///  - OnMessageReceived: nhận lại từng chuỗi
    /// </summary>
    public class NetworkManager : IDisposable
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private readonly object _sendLock = new();

        public event Action OnPeerConnected;
        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;

        public bool IsConnected =>
            _client != null &&
            _client.Connected &&
            _stream != null;

        public NetworkManager() { }

        public static bool IsNetworkAvailable()
        {
            try { return NetworkInterface.GetIsNetworkAvailable(); }
            catch { return true; }
        }

        // ================= HOST =================

        /// <summary>
        /// Host lắng nghe trên port cho 1 client duy nhất.
        /// </summary>
        public void StartHost(int port)
        {
            StopInternal();

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            Task.Run(async () =>
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    if (_cts.IsCancellationRequested) return;

                    _client = client;
                    _stream = _client.GetStream();

                    OnPeerConnected?.Invoke();
                    _ = ReceiveLoopAsync(_cts.Token);
                }
                catch
                {
                    RaiseDisconnected();
                }
            }, _cts.Token);
        }

        // ================= CLIENT =================

        /// <summary>
        /// Client kết nối tới Host qua hostIp:port.
        /// </summary>
        public async Task JoinHost(string hostIp, int port)
        {
            StopInternal();
            _cts = new CancellationTokenSource();

            _client = new TcpClient();
            await _client.ConnectAsync(hostIp, port);
            _stream = _client.GetStream();

            OnPeerConnected?.Invoke();
            _ = ReceiveLoopAsync(_cts.Token);
        }

        // ================= RECEIVE LOOP =================

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[4096];
            var sb = new StringBuilder();

            try
            {
                while (!token.IsCancellationRequested && _client != null && _client.Connected)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read <= 0) break;

                    string chunk = Encoding.UTF8.GetString(buffer, 0, read);
                    sb.Append(chunk);

                    while (true)
                    {
                        string cur = sb.ToString();
                        int idx = cur.IndexOf('\n');
                        if (idx < 0) break;

                        string line = cur.Substring(0, idx);
                        sb.Remove(0, idx + 1);

                        if (!string.IsNullOrWhiteSpace(line))
                            OnMessageReceived?.Invoke(line);
                    }
                }
            }
            catch
            {
                // ignore, sẽ raise disconnect phía dưới
            }

            RaiseDisconnected();
        }

        // ================= SEND =================

        /// <summary>
        /// Gửi 1 message (string). Hàm sẽ tự thêm '\n' làm delimiter.
        /// </summary>
        public void Send(string msg)
        {
            if (!IsConnected || string.IsNullOrEmpty(msg)) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
                lock (_sendLock)
                {
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch
            {
                RaiseDisconnected();
            }
        }

        private void RaiseDisconnected()
        {
            StopInternal();
            OnDisconnected?.Invoke();
        }

        private void StopInternal()
        {
            try { _cts?.Cancel(); } catch { }
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            try { _listener?.Stop(); } catch { }

            _cts = null;
            _stream = null;
            _client = null;
            _listener = null;
        }

        public void Dispose()
        {
            StopInternal();
        }
    }
}
