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
    /// Chat sảnh LAN + DM dùng UDP broadcast.
    /// - Port cố định: CONG_CHAT_SANH.
    /// - Gói sảnh:  LOBBY|senderId|messageId|ten|noiDung
    /// - Gói DM:    DM|senderId|messageId|from|to|noiDung
    /// - Khử trùng bằng messageId (GUID) + cache gần đây.
    /// - Chống echo nội bộ bằng senderId (GUID mỗi process).
    /// </summary>
    public class ChatSanhLAN : IDisposable
    {
        public const int CONG_CHAT_SANH = 9877;

        private static readonly string _senderId = Guid.NewGuid().ToString("N"); // duy nhất cho mỗi process

        private UdpClient? _sender;
        private UdpClient? _listener;
        private CancellationTokenSource? _ctsNghe;
        private Task? _tacVuNghe;

        // Cache khử trùng (LRU đơn giản)
        private readonly Queue<string> _recentOrder = new Queue<string>(256);
        private readonly HashSet<string> _recentIds = new HashSet<string>(StringComparer.Ordinal);
        private const int RECENT_LIMIT = 256;

        public event Action<string, string>? NhanTinSanh;           // (tenNguoi, noiDung)
        public event Action<string, string, string>? NhanTinDM;     // (fromUser, toUser, noiDung)

        // ==================== NGHE ====================
        public void BatDauNghe()
        {
            if (_listener != null) return; // đã bật

            DungNghe();

            _ctsNghe = new CancellationTokenSource();

            // Cho phép nhiều socket bind cùng 1 port (ReuseAddress)
            var udp = new UdpClient(AddressFamily.InterNetwork)
            {
                ExclusiveAddressUse = false
            };
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, CONG_CHAT_SANH));
            udp.EnableBroadcast = true;

            _listener = udp;

            _tacVuNghe = Task.Run(async () =>
            {
                var token = _ctsNghe!.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        UdpReceiveResult result = await _listener!.ReceiveAsync().ConfigureAwait(false);
                        string text = Encoding.UTF8.GetString(result.Buffer);

                        // ====== GÓI DM ======
                        if (text.StartsWith("DM|", StringComparison.Ordinal))
                        {
                            // Định dạng: DM|senderId|msgId|from|to|noiDung
                            int p0 = text.IndexOf('|');
                            int p1 = text.IndexOf('|', p0 + 1); if (p1 < 0) continue;
                            int p2 = text.IndexOf('|', p1 + 1); if (p2 < 0) continue;
                            int p3 = text.IndexOf('|', p2 + 1); if (p3 < 0) continue;
                            int p4 = text.IndexOf('|', p3 + 1); if (p4 < 0) continue;

                            string senderId = text.Substring(p0 + 1, p1 - (p0 + 1));
                            string msgId = text.Substring(p1 + 1, p2 - (p1 + 1));
                            string fromUser = text.Substring(p2 + 1, p3 - (p2 + 1));
                            string toUser = text.Substring(p3 + 1, p4 - (p3 + 1));
                            string noiDung = text.Substring(p4 + 1);

                            // Bỏ echo nội bộ
                            if (string.Equals(senderId, _senderId, StringComparison.Ordinal))
                                continue;

                            // Khử trùng
                            if (!GhiNhanNeuChuaCo(msgId))
                                continue;

                            try { NhanTinDM?.Invoke(fromUser, toUser, noiDung); } catch { }
                            continue;
                        }

                        // ====== GÓI SẢNH ======
                        // Định dạng: LOBBY|senderId|messageId|ten|noiDung
                        if (!text.StartsWith("LOBBY|", StringComparison.Ordinal)) continue;

                        int x0 = text.IndexOf('|');
                        int x1 = text.IndexOf('|', x0 + 1); if (x1 < 0) continue;
                        int x2 = text.IndexOf('|', x1 + 1); if (x2 < 0) continue;
                        int x3 = text.IndexOf('|', x2 + 1); if (x3 < 0) continue;

                        string sId = text.Substring(x0 + 1, x1 - (x0 + 1));
                        string msgId2 = text.Substring(x1 + 1, x2 - (x1 + 1));
                        string tenNguoi = text.Substring(x2 + 1, x3 - (x2 + 1));
                        string noiDung2 = text.Substring(x3 + 1);

                        if (string.Equals(sId, _senderId, StringComparison.Ordinal))
                            continue;

                        if (!GhiNhanNeuChuaCo(msgId2))
                            continue;

                        try { NhanTinSanh?.Invoke(tenNguoi, noiDung2); } catch { }
                    }
                }
                catch (ObjectDisposedException) { }
                catch (TaskCanceledException) { }
                catch { }
            }, _ctsNghe.Token);
        }

        private bool GhiNhanNeuChuaCo(string id)
        {
            lock (_recentIds)
            {
                if (_recentIds.Contains(id)) return false;

                _recentIds.Add(id);
                _recentOrder.Enqueue(id);
                if (_recentOrder.Count > RECENT_LIMIT)
                {
                    string old = _recentOrder.Dequeue();
                    _recentIds.Remove(old);
                }
                return true;
            }
        }

        public void DungNghe()
        {
            try { _ctsNghe?.Cancel(); } catch { }
            try { _listener?.Close(); } catch { }

            _tacVuNghe = null;
            _listener = null;
            _ctsNghe = null;

            lock (_recentIds)
            {
                _recentIds.Clear();
                _recentOrder.Clear();
            }
        }

        // ==================== GỬI SẢNH ====================
        public async Task GuiTinSanhAsync(string tenNguoi, string noiDung)
        {
            if (_sender == null)
            {
                _sender = new UdpClient();
                _sender.EnableBroadcast = true;
            }

            string msgId = Guid.NewGuid().ToString("N");
            string msg = $"LOBBY|{_senderId}|{msgId}|{tenNguoi}|{noiDung}";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            try
            {
                var sentTo = new HashSet<string>(StringComparer.Ordinal);
                foreach (var bcast in LayTatCaDiaChiBroadcast())
                {
                    string key = bcast.ToString();
                    if (!sentTo.Add(key)) continue;

                    try
                    {
                        var ep = new IPEndPoint(bcast, CONG_CHAT_SANH);
                        await _sender.SendAsync(data, data.Length, ep).ConfigureAwait(false);
                    }
                    catch { }
                }
            }
            catch { }
        }

        // ==================== GỬI DM ====================
        public async Task GuiTinDMAsync(string fromUser, string toUser, string noiDung)
        {
            if (_sender == null)
            {
                _sender = new UdpClient();
                _sender.EnableBroadcast = true;
            }

            string msgId = Guid.NewGuid().ToString("N");
            string msg = $"DM|{_senderId}|{msgId}|{fromUser}|{toUser}|{noiDung}";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            try
            {
                var sentTo = new HashSet<string>(StringComparer.Ordinal);
                foreach (var bcast in LayTatCaDiaChiBroadcast())
                {
                    string key = bcast.ToString();
                    if (!sentTo.Add(key)) continue;

                    try
                    {
                        var ep = new IPEndPoint(bcast, CONG_CHAT_SANH);
                        await _sender.SendAsync(data, data.Length, ep).ConfigureAwait(false);
                    }
                    catch { }
                }
            }
            catch { }
        }

        // ==================== TÍNH BROADCAST ====================
        private static List<IPAddress> LayTatCaDiaChiBroadcast()
        {
            var list = new List<IPAddress>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    if (ua.Address.Equals(IPAddress.Any) || ua.Address.Equals(IPAddress.None)) continue;

                    var mask = ua.IPv4Mask;
                    if (mask == null) continue;

                    byte[] ipBytes = ua.Address.GetAddressBytes();
                    byte[] maskBytes = mask.GetAddressBytes();
                    if (ipBytes.Length != maskBytes.Length) continue;

                    byte[] broadcast = new byte[ipBytes.Length];
                    for (int i = 0; i < ipBytes.Length; i++)
                        broadcast[i] = (byte)(ipBytes[i] | ~maskBytes[i]);

                    list.Add(new IPAddress(broadcast));
                }
            }

            if (list.Count == 0) list.Add(IPAddress.Broadcast);
            return list;
        }

        public void Dispose()
        {
            DungNghe();
            try { _sender?.Close(); } catch { }
            _sender = null;
        }
    }
}
