using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace plan_fighting_super_start
{
    /// <summary>
    /// LANBroadcast:
    ///  - Host: StartBroadcast(roomId, gamePort)
    ///  - Client: StartListen(roomIdFilter)
    ///  - OnRoomFound: bắn ra khi thấy phòng
    ///
    /// Bản này:
    ///  - Gửi broadcast trên TẤT CẢ card LAN/Wi-Fi IPv4 "thật"
    ///  - Thêm 255.255.255.255 + 127.0.0.1
    /// => Hạn chế tối đa vụ "máy này thấy phòng, máy kia không".
    /// </summary>
    public class LANBroadcast : IDisposable
    {
        public const int BROADCAST_PORT = 9876;

        private UdpClient _sender;
        private UdpClient _listener;
        private CancellationTokenSource _ctsBroadcast;
        private CancellationTokenSource _ctsListen;

        /// <summary>
        /// roomId, hostIP, port
        /// </summary>
        public event Action<string, string, int> OnRoomFound;

        // ================= HOST – BROADCAST =================

        /// <summary>
        /// Host spam UDP thông tin phòng theo chu kỳ.
        /// </summary>
        public void StartBroadcast(string roomId, int gamePort, int intervalMs = 1000)
        {
            StopBroadcast();

            _ctsBroadcast = new CancellationTokenSource();
            _sender = new UdpClient();
            _sender.EnableBroadcast = true;

            Task.Run(async () =>
            {
                var token = _ctsBroadcast.Token;

                // Lấy tất cả broadcast address có thể có trên máy
                var broadcastAddrs = GetLanBroadcastAddresses();
                var broadcastTargets = new List<IPEndPoint>();

                foreach (var ip in broadcastAddrs)
                    broadcastTargets.Add(new IPEndPoint(ip, BROADCAST_PORT));

                // Giữ loopback để debug nhiều client trên 1 máy
                broadcastTargets.Add(new IPEndPoint(IPAddress.Loopback, BROADCAST_PORT));

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        string payload = $"ROOM:{roomId};PORT:{gamePort}";
                        byte[] data = Encoding.UTF8.GetBytes(payload);

                        foreach (var ep in broadcastTargets)
                        {
                            try
                            {
                                await _sender.SendAsync(data, data.Length, ep);
                            }
                            catch
                            {
                                // bỏ qua từng endpoint lỗi, tiếp tục vòng
                            }
                        }
                    }
                    catch
                    {
                        // ignore vòng này
                    }

                    try
                    {
                        await Task.Delay(intervalMs, token);
                    }
                    catch
                    {
                        // bị cancel
                        break;
                    }
                }
            }, _ctsBroadcast.Token);
        }

        public void StopBroadcast()
        {
            try { _ctsBroadcast?.Cancel(); } catch { }
            try { _sender?.Close(); } catch { }
            _ctsBroadcast = null;
            _sender = null;
        }

        // ================= CLIENT – LISTEN =================

        public void StartListen(string roomIdFilter)
        {
            StopListen();
            _ctsListen = new CancellationTokenSource();

            // Dùng Socket để set ReuseAddress
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try { sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); } catch { }
            try { sock.ExclusiveAddressUse = false; } catch { }
            sock.Bind(new IPEndPoint(IPAddress.Any, BROADCAST_PORT));
            _listener = new UdpClient { Client = sock };

            Task.Run(async () =>
            {
                var token = _ctsListen.Token;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _listener.ReceiveAsync();
                        string msg = Encoding.UTF8.GetString(result.Buffer);

                        if (!msg.StartsWith("ROOM:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string roomId = null;
                        int port = -1;

                        var parts = msg.Split(';');
                        foreach (var p in parts)
                        {
                            var kv = p.Split(':');
                            if (kv.Length != 2) continue;
                            string k = kv[0].Trim().ToUpperInvariant();
                            string v = kv[1].Trim();
                            if (k == "ROOM") roomId = v;
                            else if (k == "PORT" && int.TryParse(v, out int prt)) port = prt;
                        }

                        if (roomId == null || port <= 0) continue;
                        if (!string.Equals(roomId, roomIdFilter, StringComparison.Ordinal))
                            continue;

                        string hostIP = result.RemoteEndPoint.Address.ToString();
                        OnRoomFound?.Invoke(roomId, hostIP, port);
                    }
                    catch
                    {
                        // Nếu bị dispose hoặc cancel thì thoát vòng
                        if (_ctsListen == null || _ctsListen.IsCancellationRequested)
                            break;
                    }
                }
            }, _ctsListen.Token);
        }

        public void StopListen()
        {
            try { _ctsListen?.Cancel(); } catch { }
            try { _listener?.Close(); } catch { }
            _ctsListen = null;
            _listener = null;
        }

        // ================= DISPOSE =================

        public void Dispose()
        {
            StopBroadcast();
            StopListen();
        }

        // ================= HELPER – MULTI NIC =================

        /// <summary>
        /// Lấy tất cả broadcast IPv4 từ các card LAN/Wi-Fi "thật" + 255.255.255.255
        /// </summary>
        private List<IPAddress> GetLanBroadcastAddresses()
        {
            var list = new List<IPAddress>();
            var seen = new HashSet<string>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                // Bỏ loopback, tunnel
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                var name = (ni.Name + " " + ni.Description).ToLowerInvariant();

                // Tùy bạn: có thể bỏ card ảo thường gặp
                if (name.Contains("vmware") ||
                    name.Contains("virtualbox") ||
                    name.Contains("virtual") ||
                    name.Contains("vpn") ||
                    name.Contains("hyper-v"))
                {
                    continue;
                }

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (ua.IPv4Mask == null)
                        continue;

                    var broadcast = GetBroadcastAddress(ua.Address, ua.IPv4Mask);
                    var key = broadcast.ToString();
                    if (!seen.Contains(key))
                    {
                        seen.Add(key);
                        list.Add(broadcast);
                    }
                }
            }

            // Thêm global broadcast cuối cùng
            if (!seen.Contains(IPAddress.Broadcast.ToString()))
                list.Add(IPAddress.Broadcast);

            return list;
        }

        private IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipBytes = address.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            if (ipBytes.Length != maskBytes.Length)
                throw new ArgumentException("IP và subnet mask không cùng chiều dài");

            byte[] broadcast = new byte[ipBytes.Length];
            for (int i = 0; i < broadcast.Length; i++)
            {
                broadcast[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 255));
            }

            return new IPAddress(broadcast);
        }
    }
}
