using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class ChatRieng : Form
    {
        private readonly string _ban;   // username người bên kia
        private readonly string _toi;   // username của mình
        private ChatSanhLAN? _kenhLan;

        public ChatRieng(string tenBan)
        {
            _ban = tenBan ?? "";
            _toi = AccountData.Username ?? "me";

            InitializeComponent();
        }

        // ================== SỰ KIỆN FORM ==================

        private async void ChatRieng_Load(object sender, EventArgs e)
        {
            lblTieuDe.Text = $"NHẮN VỚI  {_ban.ToUpper()}";

            // Bật nghe LAN DM
            _kenhLan = new ChatSanhLAN();
            _kenhLan.BatDauNghe();
            _kenhLan.NhanTinDM += XuLyTinNhanDM;

            // 🔹 Khi mở form, tải lịch sử chat từ S3 qua API
            await TaiLichSuCloudAsync();
        }

        private void ChatRieng_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (_kenhLan != null)
                {
                    _kenhLan.NhanTinDM -= XuLyTinNhanDM;
                    _kenhLan.Dispose();
                }
            }
            catch
            {
                // ignore
            }
        }

        // ================== NHẬN TIN TỪ LAN ==================

        // Hàm này được ChatSanhLAN gọi khi có DM tới
        private void XuLyTinNhanDM(string tu, string den, string noiDung)
        {
            // Chỉ nhận tin thuộc cặp (_toi <-> _ban)
            bool dungCuoc =
                (string.Equals(tu, _ban, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(den, _toi, StringComparison.OrdinalIgnoreCase))
             || (string.Equals(tu, _toi, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(den, _ban, StringComparison.OrdinalIgnoreCase));

            if (!dungCuoc) return;

            bool laCuaToi = string.Equals(tu, _toi, StringComparison.OrdinalIgnoreCase);
            ChenDong(tu, noiDung, laCuaToi);

        }

        // ================== GỬI TIN ==================

        private async void btnGui_Click(object sender, EventArgs e)
        {
            string msg = (txtNoiDung.Text ?? "").Trim();
            if (msg.Length == 0) return;

            try
            {
                if (_kenhLan == null)
                {
                    _kenhLan = new ChatSanhLAN();
                    _kenhLan.BatDauNghe();
                    _kenhLan.NhanTinDM += XuLyTinNhanDM;
                }

                // Gửi qua LAN
                await _kenhLan.GuiTinDMAsync(_toi, _ban, msg);
            }
            catch
            {
                // tránh crash nếu LAN lỗi
            }

            // Hiện luôn tin của mình
            ChenDong(_toi, msg, true);
            txtNoiDung.Clear();

            // 🔹 Ghi tin nhắn này lên S3 qua API (append dòng mới)
            _ = LuuTinNhanCloudAsync(_toi, _ban, msg);
        }

        // ================== LÀM VIỆC VỚI API (S3) ==================

        private async Task LuuTinNhanCloudAsync(string from, string to, string msg)
        {
            try
            {
                await DmApiClient.AppendMessageAsync(from, to, msg);
            }
            catch
            {
            }
        }

        private async Task TaiLichSuCloudAsync()
        {
            try
            {
                var res = await DmApiClient.GetHistoryAsync(_toi, _ban);
                if (res?.Lines == null || res.Lines.Length == 0)
                    return;

                rtbHopThoai.Clear();

                foreach (var line in res.Lines)
                {
                    // format: time|from|to|message
                    var parts = line.Split('|', 4);
                    if (parts.Length < 4) continue;

                    string from = parts[1];
                    string to = parts[2];
                    string message = parts[3];

                    bool laCuaToi = string.Equals(from, _toi, StringComparison.OrdinalIgnoreCase);
                    ChenDong(from, message, laCuaToi);
                }
            }
            catch
            {
            }
        }

        // ================== HIỂN THỊ LÊN RICH TEXT BOX ==================

        // Thêm 1 dòng vào khung chat
        private void ChenDong(string ai, string text, bool laCuaToi)
        {
            if (rtbHopThoai.InvokeRequired)
            {
                rtbHopThoai.Invoke(new Action(() => ChenDong(ai, text, laCuaToi)));
                return;
            }

            if (rtbHopThoai.TextLength > 0)
                rtbHopThoai.AppendText(Environment.NewLine);

            string nhan = laCuaToi ? "[TÔI] " : "[BẠN] ";
            Color mauNhan = laCuaToi ? Color.Lime : Color.Cyan;
            Color mauTen = laCuaToi ? Color.White : Color.Gold;

            int start = rtbHopThoai.TextLength;

            // [TÔI] / [BẠN]
            rtbHopThoai.AppendText(nhan);
            rtbHopThoai.Select(start, nhan.Length);
            rtbHopThoai.SelectionColor = mauNhan;

            // Tên
            string phanTen = ai + ": ";
            rtbHopThoai.AppendText(phanTen);
            rtbHopThoai.Select(start + nhan.Length, phanTen.Length);
            rtbHopThoai.SelectionColor = mauTen;

            // Nội dung
            rtbHopThoai.AppendText(text);
            rtbHopThoai.SelectionColor = Color.White;

            // Cuộn xuống cuối
            rtbHopThoai.SelectionStart = rtbHopThoai.TextLength;
            rtbHopThoai.ScrollToCaret();
        }
    }
}
