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
   
    public class ChatSanhLAN : IDisposable
    {
        public const int CONG_CHAT_SANH = 9877;

        private static readonly string _senderId = Guid.NewGuid().ToString("N"); // duy nhất cho mỗi process

        private UdpClient _sender;
        private UdpClient _listener;
        private CancellationTokenSource _ctsNghe;
        private Task _tacVuNghe;

        // Cache khử trùng (LRU đơn giản)
        private readonly Queue<string> _recentOrder = new Queue<string>(256);
        private readonly HashSet<string> _recentIds = new HashSet<string>(StringComparer.Ordinal);
        private const int RECENT_LIMIT = 256;

        public event Action<string, string> NhanTinSanh; // (tenNguoi, noiDung)

        // ==================== NGHE ====================
        public void BatDauNghe()
        {
            if (_listener != null) return; // đã bật

            DungNghe();

            _ctsNghe = new CancellationTokenSource();

            // Cho phép nhiều socket bind cùng port
            var udp = new UdpClient(AddressFamily.InterNetwork);
            udp.ExclusiveAddressUse = false;
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, CONG_CHAT_SANH));
            udp.EnableBroadcast = true;

            _listener = udp;

            _tacVuNghe = Task.Run(async () =>
            {
                var token = _ctsNghe.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        UdpReceiveResult result = await _listener.ReceiveAsync().ConfigureAwait(false);
                        string text = Encoding.UTF8.GetString(result.Buffer);

                        // Định dạng: LOBBY|senderId|messageId|ten|noiDung
                        // Tách nhanh bằng IndexOf để chịu được '|' trong nội dung
                        int p0 = text.IndexOf('|'); if (p0 <= 0 || !text.StartsWith("LOBBY|")) continue;
                        int p1 = text.IndexOf('|', p0 + 1); if (p1 < 0) continue;
                        int p2 = text.IndexOf('|', p1 + 1); if (p2 < 0) continue;
                        int p3 = text.IndexOf('|', p2 + 1); if (p3 < 0) continue;

                        string senderId = text.Substring(p0 + 1, p1 - (p0 + 1));
                        string msgId = text.Substring(p1 + 1, p2 - (p1 + 1));
                        string tenNguoi = text.Substring(p2 + 1, p3 - (p2 + 1));
                        string noiDung = text.Substring(p3 + 1);

                        // 1) Bỏ gói của chính process này (echo nội bộ)
                        if (string.Equals(senderId, _senderId, StringComparison.Ordinal))
                            continue;

                        // 2) Khử trùng cùng messageId (nhận qua nhiều NIC)
                        if (!GhiNhanNeuChuaCo(msgId))
                            continue;

                        NhanTinSanh?.Invoke(tenNguoi, noiDung);
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
                    var old = _recentOrder.Dequeue();
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

            lock (_recentIds) { _recentIds.Clear(); _recentOrder.Clear(); }
        }

        // ==================== GỬI ====================
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
                // Gửi tới tất cả broadcast của từng NIC (loại trùng bằng HashSet)
                var sentTo = new HashSet<string>(StringComparer.Ordinal);
                foreach (var bcast in LayTatCaDiaChiBroadcast())
                {
                    string key = bcast.ToString();
                    if (!sentTo.Add(key)) continue; // tránh gửi trùng cùng địa chỉ

                    try
                    {
                        var ep = new IPEndPoint(bcast, CONG_CHAT_SANH);
                        await _sender.SendAsync(data, data.Length, ep).ConfigureAwait(false);
                    }
                    catch { /* tiếp tục với NIC khác */ }
                }
            }
            catch
            {
                // ignore
            }
        }

        // ==================== UTIL ====================
        private static IEnumerable<IPAddress> LayTatCaDiaChiBroadcast()
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
