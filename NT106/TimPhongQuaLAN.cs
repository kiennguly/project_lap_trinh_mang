using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    /// <summary>
    /// Tìm phòng LAN bằng UDP broadcast.
    /// - Máy Host: phát thông tin phòng (Mã phòng + Port) định kỳ.
    /// - Máy Client: lắng nghe broadcast, tìm đúng Mã phòng rồi gọi callback trả về IP host + port.
    /// </summary>
    public class TimPhongQuaLAN : IDisposable
    {
        public const int CONG_BROADCAST = 9876;

        private UdpClient _sender;
        private CancellationTokenSource _ctsPhatPhong;
        private Task _tacVuPhatPhong;

        private UdpClient _listener;
        private CancellationTokenSource _ctsNghePhong;
        private Task _tacVuNghePhong;

        // ==================== HOST: PHÁT PHÒNG ====================
        /// <summary>
        /// Host bắt đầu phát thông tin phòng ra LAN.
        /// </summary>
        public void BatDauPhatPhong(string maPhong, int port, int chuKyMs = 1000)
        {
            DungPhatPhong();

            _ctsPhatPhong = new CancellationTokenSource();
            _sender = new UdpClient();
            _sender.EnableBroadcast = true;

            _tacVuPhatPhong = Task.Run(async () =>
            {
                var token = _ctsPhatPhong.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        string msg = $"ROOM:{maPhong};PORT:{port}";
                        byte[] data = Encoding.UTF8.GetBytes(msg);

                        foreach (var addr in LayTatCaDiaChiBroadcast())
                        {
                            try
                            {
                                var ep = new IPEndPoint(addr, CONG_BROADCAST);
                                await _sender.SendAsync(data, data.Length, ep).ConfigureAwait(false);
                            }
                            catch
                            {
                                // bỏ qua lỗi từng interface
                            }
                        }

                        await Task.Delay(chuKyMs, token).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                catch
                {
                    // ignore
                }
            }, _ctsPhatPhong.Token);
        }

        public void DungPhatPhong()
        {
            try { _ctsPhatPhong?.Cancel(); } catch { }
            try { _sender?.Close(); } catch { }

            _tacVuPhatPhong = null;
            _sender = null;
            _ctsPhatPhong = null;
        }

        // ==================== CLIENT: TÌM PHÒNG ====================
        /// <summary>
        /// Client lắng nghe broadcast, khi tìm thấy phòng có mã trùng thì gọi callback(hostIp, port).
        /// </summary>
        public void BatDauTimPhong(string maPhongCanTim, Action<string, int> khiTimThayPhong)
        {
            DungTimPhong();

            _ctsNghePhong = new CancellationTokenSource();
            _listener = new UdpClient(CONG_BROADCAST);
            _listener.EnableBroadcast = true;

            _tacVuNghePhong = Task.Run(async () =>
            {
                var token = _ctsNghePhong.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        UdpReceiveResult result = await _listener.ReceiveAsync().ConfigureAwait(false);
                        string text = Encoding.UTF8.GetString(result.Buffer);

                        // Format: ROOM:123456;PORT:8888
                        string maPhongNhan = null;
                        int portNhan = -1;

                        foreach (var part in text.Split(';'))
                        {
                            var kv = part.Split(':');
                            if (kv.Length != 2) continue;

                            string key = kv[0].Trim().ToUpperInvariant();
                            string val = kv[1].Trim();

                            if (key == "ROOM") maPhongNhan = val;
                            else if (key == "PORT" && int.TryParse(val, out int p)) portNhan = p;
                        }

                        if (maPhongNhan == null || portNhan <= 0) continue;
                        if (!string.Equals(maPhongNhan, maPhongCanTim, StringComparison.Ordinal)) continue;

                        string ipHost = result.RemoteEndPoint.Address.ToString();
                        khiTimThayPhong?.Invoke(ipHost, portNhan);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // socket đóng
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                catch
                {
                    // ignore
                }
            }, _ctsNghePhong.Token);
        }

        public void DungTimPhong()
        {
            try { _ctsNghePhong?.Cancel(); } catch { }
            try { _listener?.Close(); } catch { }

            _tacVuNghePhong = null;
            _listener = null;
            _ctsNghePhong = null;
        }

        // ==================== UTIL: LẤY ĐỊA CHỈ BROADCAST ====================
        private IEnumerable<IPAddress> LayTatCaDiaChiBroadcast()
        {
            var list = new List<IPAddress>();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    var mask = ua.IPv4Mask;
                    if (mask == null) continue;

                    byte[] ipBytes = ua.Address.GetAddressBytes();
                    byte[] maskBytes = mask.GetAddressBytes();
                    if (ipBytes.Length != maskBytes.Length) continue;

                    byte[] broadcast = new byte[ipBytes.Length];
                    for (int i = 0; i < ipBytes.Length; i++)
                    {
                        broadcast[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                    }

                    list.Add(new IPAddress(broadcast));
                }
            }

            // fallback
            if (list.Count == 0)
                list.Add(IPAddress.Broadcast);

            return list;
        }

        public void Dispose()
        {
            DungPhatPhong();
            DungTimPhong();
        }
    }
}
