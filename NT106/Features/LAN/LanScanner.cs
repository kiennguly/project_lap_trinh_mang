using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    /// <summary>
    /// LanScanner:
    ///  - Tự quét toàn bộ dải /24 của card LAN/Wi-Fi chính
    ///  - Không cần user nhập IP
    ///  - Dùng TCP gamePort để handshake:
    ///      Client gửi:  "DISCOVER_REQUEST"
    ///      Host trả:    "DISCOVER_RESPONSE;ROOM:...;NAME:...;PORT:..."
    /// </summary>
    public class LanScanner : IDisposable
    {
        private readonly int _gamePort;
        private CancellationTokenSource _cts;
        private bool _scanning;

        /// <summary>
        /// ip, roomId, port, hostName
        /// </summary>
        public event Action<string, string, int, string> OnHostFound;

        public LanScanner(int gamePort)
        {
            _gamePort = gamePort;
        }

        /// <summary>
        /// Bắt đầu scan LAN (non-blocking). Có thể gọi lại để scan mới.
        /// </summary>
        public void StartScan(int timeoutPerHostMs = 200, int maxConcurrency = 32)
        {
            StopScan();

            _cts = new CancellationTokenSource();
            _scanning = true;

            Task.Run(() => ScanLanAsync(timeoutPerHostMs, maxConcurrency, _cts.Token));
        }

        public void StopScan()
        {
            try { _cts?.Cancel(); } catch { }
            _cts = null;
            _scanning = false;
        }

        public void Dispose()
        {
            StopScan();
        }

        // ================= CORE SCAN =================

        private async Task ScanLanAsync(int timeoutPerHostMs, int maxConcurrency, CancellationToken token)
        {
            try
            {
                var hosts = GetHostCandidates();
                if (hosts.Count == 0) return;

                var tasks = new List<Task>();
                var sem = new SemaphoreSlim(maxConcurrency);

                foreach (var ip in hosts)
                {
                    if (token.IsCancellationRequested) break;

                    await sem.WaitAsync(token).ConfigureAwait(false);

                    var t = ProbeHostAsync(ip, timeoutPerHostMs, token)
                        .ContinueWith(_ => sem.Release(), TaskScheduler.Default);

                    tasks.Add(t);
                }

                await Task.WhenAll(tasks);
            }
            catch
            {
                // ignore
            }
            finally
            {
                _scanning = false;
            }
        }

        private async Task ProbeHostAsync(string ip, int timeoutMs, CancellationToken token)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, _gamePort);

                // Timeout cho connect
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, token));
                if (completed != connectTask || token.IsCancellationRequested)
                    return; // timeout hoặc cancel

                if (!client.Connected) return;

                using var stream = client.GetStream();

                // Gửi DISCOVER_REQUEST
                byte[] req = Encoding.UTF8.GetBytes("DISCOVER_REQUEST\n");
                await stream.WriteAsync(req, 0, req.Length, token);

                // Đợi trả lời
                var buffer = new byte[512];
                var readTask = stream.ReadAsync(buffer, 0, buffer.Length, token);
                completed = await Task.WhenAny(readTask, Task.Delay(timeoutMs, token));
                if (completed != readTask || token.IsCancellationRequested)
                    return; // timeout

                int read = readTask.Result;
                if (read <= 0) return;

                string resp = Encoding.UTF8.GetString(buffer, 0, read).Trim();

                if (!resp.StartsWith("DISCOVER_RESPONSE", StringComparison.OrdinalIgnoreCase))
                    return;

                // Ví dụ format:
                // DISCOVER_RESPONSE;ROOM:room123;NAME:Minh;PORT:9000
                string roomId = null;
                string hostName = "";
                int port = _gamePort;

                var parts = resp.Split(';');
                foreach (var p in parts)
                {
                    var kv = p.Split(':');
                    if (kv.Length != 2) continue;
                    string k = kv[0].Trim().ToUpperInvariant();
                    string v = kv[1].Trim();

                    if (k == "ROOM") roomId = v;
                    else if (k == "NAME") hostName = v;
                    else if (k == "PORT" && int.TryParse(v, out var prt)) port = prt;
                }

                if (roomId == null) return;

                OnHostFound?.Invoke(ip, roomId, port, hostName);
            }
            catch
            {
                // im lặng, host không online hoặc không phải game
            }
        }

        // ================= HELPER: Lấy dải IP /24 =================

        private List<string> GetHostCandidates()
        {
            var result = new List<string>();

            // Chọn card ưu tiên: Wi-Fi rồi Ethernet
            UnicastIPAddressInformation chosen = null;

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (ua.IPv4Mask == null)
                        continue;

                    chosen = ua;
                    break;
                }

                if (chosen != null)
                    break;
            }

            if (chosen == null)
                return result;

            var ipBytes = chosen.Address.GetAddressBytes();

            // Giả sử /24: dùng 3 byte đầu, quét 1..254
            byte[] baseNet = { ipBytes[0], ipBytes[1], ipBytes[2], 0 };
            string selfIp = chosen.Address.ToString();

            for (int i = 1; i <= 254; i++)
            {
                baseNet[3] = (byte)i;
                string candidate = new IPAddress(baseNet).ToString();

                if (candidate == selfIp) continue; // bỏ IP của chính mình

                result.Add(candidate);
            }

            return result;
        }
    }
}
