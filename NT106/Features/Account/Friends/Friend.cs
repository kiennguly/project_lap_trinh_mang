using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Friend : Form
    {
        // Có thể tắt avatar nếu thấy chậm
        private const bool LOAD_AVATAR = true;
        private const int AVATAR_TIMEOUT_MS = 800;

        private readonly ImageList _avatars = new ImageList();
        private S3ImageService? _s3;
        private List<FriendEntry> _friends = new List<FriendEntry>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isLoading = false;

        // Đảm bảo không gắn event cho ListView nhiều lần
        private bool _listViewConfigured = false;

        public Friend()
        {
            InitializeComponent();

            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _s3 = new S3ImageService();
            ForceListViewConfig();
            SetupColumns();
        }

        // ==================== Cấu hình ListView ====================

        private void ForceListViewConfig()
        {
            if (_listViewConfigured || lvFriends == null) return;
            _listViewConfigured = true;

            lvFriends.View = View.Details;
            lvFriends.FullRowSelect = true;
            lvFriends.MultiSelect = false;
            lvFriends.GridLines = false;
            lvFriends.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvFriends.UseCompatibleStateImageBehavior = false;

            _avatars.ColorDepth = ColorDepth.Depth32Bit;
            _avatars.ImageSize = new Size(48, 48);
            lvFriends.SmallImageList = _avatars;

            lvFriends.OwnerDraw = true;
            lvFriends.DrawColumnHeader += Lv_DrawColumnHeader;
            lvFriends.DrawItem += Lv_DrawItem;
            lvFriends.DrawSubItem += Lv_DrawSubItem;
            lvFriends.MouseUp += Lv_MouseUp;
        }

        private void SetupColumns()
        {
            try
            {
                lvFriends.Columns.Clear();
                // 0: User (avatar + tên)
                lvFriends.Columns.Add("User", 260, HorizontalAlignment.Left);
                // 1: Status
                lvFriends.Columns.Add("Status", 220, HorizontalAlignment.Left);
                // 2: Chat icon
                lvFriends.Columns.Add("", 60, HorizontalAlignment.Center);
            }
            catch { }
        }

        private bool UiAlive()
            => !(IsDisposed || !IsHandleCreated || lvFriends == null || lvFriends.IsDisposed);

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { _cts.Cancel(); } catch { }
            base.OnFormClosing(e);
        }

        // ==================== Form Load ====================

        private void Friend_Load(object sender, EventArgs e)
        {
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            SetupColumns();
            _ = LoadFriendsAsync();
        }

        // ==================== Tải danh sách bạn bè ====================

        private async Task LoadFriendsAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try { _cts.Cancel(); } catch { }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            btnRefresh.Enabled = false;
            btnAccept.Enabled = false;
            btnDecline.Enabled = false;
            if (UiAlive()) lblLoading.Visible = true;

            if (string.IsNullOrWhiteSpace(AccountData.Username))
            {
                if (UiAlive())
                    MessageBox.Show("Bạn cần đăng nhập trước khi xem danh sách bạn bè.", "Friend",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (UiAlive()) lblLoading.Visible = false;
                btnRefresh.Enabled = true;
                _isLoading = false;
                return;
            }

            List<FriendEntry> list;
            try
            {
                list = await Database.GetFriendListAsync(AccountData.Username);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _isLoading = false;
                return;
            }
            catch (Exception ex)
            {
                if (UiAlive())
                    MessageBox.Show("Lỗi khi tải danh sách bạn bè: " + ex.Message,
                        "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (UiAlive()) lblLoading.Visible = false;
                btnRefresh.Enabled = true;
                _isLoading = false;
                return;
            }

            _friends = list ?? new List<FriendEntry>();

            if (!UiAlive())
            {
                _isLoading = false;
                return;
            }

            try { lvFriends.BeginUpdate(); } catch { }
            try
            {
                _avatars.Images.Clear();
                lvFriends.Items.Clear();

                foreach (var f in _friends)
                {
                    // 3 cột: User, Status, Chat
                    var item = new ListViewItem(""); // User (ảnh + tên)
                    item.SubItems.Add(StatusToVietnamese(f.Status)); // Status
                    item.SubItems.Add("");                             // Chat icon
                    item.Tag = f;
                    lvFriends.Items.Add(item);
                }
            }
            finally
            {
                try { lvFriends.EndUpdate(); } catch { }
            }

            btnAccept.Enabled = false;
            btnDecline.Enabled = false;
            if (UiAlive()) lblLoading.Visible = false;
            btnRefresh.Enabled = true;
            _isLoading = false;

            if (LOAD_AVATAR)
                _ = TaiAvatarSongSongAsync(token);
        }

        private string StatusToVietnamese(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "pending": return "Đang chờ bạn chấp nhận";
                case "sent": return "Đã gửi lời mời";
                case "accepted": return "Bạn bè";
                default: return status ?? "";
            }
        }

        // ==================== Avatar song song ====================

        private async Task TaiAvatarSongSongAsync(CancellationToken token)
        {
            if (_friends.Count == 0 || !UiAlive()) return;

            using var gate = new SemaphoreSlim(6);
            var tasks = new List<Task>();

            for (int i = 0; i < _friends.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    await gate.WaitAsync(token);
                    try
                    {
                        var f = _friends[index];
                        int imgIdx = await DownloadAvatarAsync(f, token);

                        if (imgIdx >= 0 && UiAlive())
                        {
                            lvFriends.BeginInvoke(new Action(() =>
                            {
                                if (index < lvFriends.Items.Count)
                                {
                                    lvFriends.Items[index].ImageIndex = imgIdx;
                                    lvFriends.Invalidate(lvFriends.Items[index].Bounds);
                                }
                            }));
                        }
                    }
                    catch { }
                    finally
                    {
                        try { gate.Release(); } catch { }
                    }
                }, token));
            }

            try { await Task.WhenAll(tasks); } catch { }
        }

        private async Task<int> DownloadAvatarAsync(FriendEntry f, CancellationToken token)
        {
            try
            {
                if (_s3 == null) _s3 = new S3ImageService();
                string key =
                    !string.IsNullOrWhiteSpace(f.AvatarKey)
                        ? f.AvatarKey
                        : $"avatars/avatars/{f.Username}.png";

                var imgTask = _s3.GetImageAsync(key);
                var done = await Task.WhenAny(imgTask, Task.Delay(AVATAR_TIMEOUT_MS, token));
                if (done != imgTask) return -1;

                var img = await imgTask;
                if (img == null) return -1;

                if (!UiAlive()) return -1;

                int idx = -1;
                lvFriends.Invoke(new Action(() =>
                {
                    _avatars.Images.Add(img);
                    idx = _avatars.Images.Count - 1;
                }));
                return idx;
            }
            catch
            {
                return -1;
            }
        }


        // ==================== OwnerDraw ====================

        private void Lv_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(18, 26, 48)), e.Bounds);

            using var f = new Font("Consolas", 11, FontStyle.Bold);
            string text = e.ColumnIndex switch
            {
                0 => "User",
                1 => "Status",
                _ => ""
            };
            TextRenderer.DrawText(e.Graphics, text, f, e.Bounds, Color.Cyan,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void Lv_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            var back = e.Item.Selected
                ? Color.FromArgb(35, 60, 110)
                : Color.FromArgb(10, 16, 32);
            e.Graphics.FillRectangle(new SolidBrush(back), e.Bounds);
        }

        private void Lv_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item.Tag is not FriendEntry entry)
            {
                e.DrawDefault = true;
                return;
            }

            bool isFriend = string.Equals(entry.Status, "accepted", StringComparison.OrdinalIgnoreCase);

            if (e.ColumnIndex == 0)
            {
                // Avatar + username
                var r = e.Bounds;
                int left = r.Left + 8;
                int imgW = 44, imgH = 44;

                if (e.Item.ImageIndex >= 0 && e.Item.ImageIndex < _avatars.Images.Count)
                {
                    var img = _avatars.Images[e.Item.ImageIndex];
                    e.Graphics.DrawImage(
                        img,
                        new Rectangle(left, r.Top + (r.Height - imgH) / 2, imgW, imgH));
                }
                left += imgW + 8;

                using var f = new Font("Segoe UI", 10, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, entry.Username, f,
                    new Rectangle(left, r.Top, r.Right - left, r.Height),
                    Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                return;
            }

            if (e.ColumnIndex == 1)
            {
                using var f = new Font("Segoe UI", 10, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, f, e.Bounds,
                    Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                return;
            }

            // Cột 2: icon Chat
            if (e.ColumnIndex == 2)
            {
                var iconRect = e.Bounds;
                int size = Math.Min(iconRect.Height - 8, 26);
                iconRect = new Rectangle(
                    iconRect.Left + (iconRect.Width - size) / 2,
                    iconRect.Top + (iconRect.Height - size) / 2,
                    size, size);

                DrawChatIcon(e.Graphics, iconRect, isFriend);
            }
        }

        private void DrawChatIcon(Graphics g, Rectangle r, bool enabled)
        {
            var body = enabled ? Color.FromArgb(180, 220, 245) : Color.FromArgb(80, 90, 110);
            using var b = new SolidBrush(body);
            using var pen = new Pen(Color.FromArgb(60, 80, 120), 2);

            var bubble = new Rectangle(r.Left, r.Top, r.Width - r.Width / 5, r.Height - r.Height / 5);
            g.FillEllipse(b, bubble);
            g.DrawEllipse(pen, bubble);

            var tail = new Point[]
            {
                new Point(bubble.Right - 6, bubble.Bottom - 6),
                new Point(bubble.Right + 2, bubble.Bottom + 1),
                new Point(bubble.Right - 10, bubble.Bottom - 2),
            };
            g.FillPolygon(b, tail);
            g.DrawPolygon(pen, tail);
        }

        // ==================== Click icon ====================

        private void Lv_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!UiAlive()) return;

            var hit = lvFriends.HitTest(e.Location);
            if (hit.Item == null || hit.SubItem == null) return;
            if (hit.Item.Tag is not FriendEntry entry) return;

            int colIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
            bool isFriend = string.Equals(entry.Status, "accepted", StringComparison.OrdinalIgnoreCase);
            if (!isFriend) return;

            // chỉ còn 1 cột icon Chat (index = 2)
            if (colIndex == 2)
            {
                var dm = new ChatRieng(entry.Username);
                dm.Show();
            }
        }

        // ==================== Các nút ====================

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadFriendsAsync();
        }

        private async void btnSendRequest_Click(object sender, EventArgs e)
        {
            string target = (txtFriendUsername.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(target))
            {
                MessageBox.Show("Nhập username bạn bè.");
                return;
            }

            if (string.IsNullOrWhiteSpace(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập.");
                return;
            }

            if (target.Equals(AccountData.Username, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể gửi lời mời cho chính mình.");
                return;
            }

            if (_friends.Any(f => f.Username.Equals(target, StringComparison.OrdinalIgnoreCase)
                               && f.Status.Equals("accepted", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Hai bạn đã là bạn bè rồi, không thể gửi lời mời nữa.");
                return;
            }

            if (_friends.Any(f => f.Username.Equals(target, StringComparison.OrdinalIgnoreCase)
                               && (f.Status.Equals("pending", StringComparison.OrdinalIgnoreCase)
                                   || f.Status.Equals("sent", StringComparison.OrdinalIgnoreCase))))
            {
                MessageBox.Show("Bạn đã có lời mời đang xử lý với người này.");
                return;
            }

            btnSendRequest.Enabled = false;

            try
            {
                bool exists = await Database.CheckAccountExistsAsync(target);
                if (!exists)
                {
                    MessageBox.Show($"Tài khoản \"{target}\" không tồn tại.");
                    return;
                }

                bool ok = await Database.SendFriendRequestAsync(AccountData.Username, target);
                if (ok)
                {
                    txtFriendUsername.Clear();
                    await LoadFriendsAsync();
                }
                else
                {
                    MessageBox.Show("Gửi lời mời thất bại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi lời mời: " + ex.Message);
            }
            finally
            {
                btnSendRequest.Enabled = true;
            }
        }

        private void lvFriends_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UiAlive()) return;

            if (lvFriends.SelectedItems.Count == 0)
            {
                btnAccept.Enabled = false;
                btnDecline.Enabled = false;
                return;
            }

            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            bool canResp = entry != null &&
                           string.Equals(entry.Status, "pending", StringComparison.OrdinalIgnoreCase);
            btnAccept.Enabled = canResp;
            btnDecline.Enabled = canResp;
        }

        private async void btnAccept_Click(object sender, EventArgs e)
        {
            if (lvFriends.SelectedItems.Count == 0) return;
            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            if (entry == null) return;

            btnAccept.Enabled = btnDecline.Enabled = false;
            await Database.RespondFriendRequestAsync(entry.Username, AccountData.Username, true);
            await LoadFriendsAsync();
        }

        private async void btnDecline_Click(object sender, EventArgs e)
        {
            if (lvFriends.SelectedItems.Count == 0) return;
            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            if (entry == null) return;

            btnAccept.Enabled = btnDecline.Enabled = false;
            await Database.RespondFriendRequestAsync(entry.Username, AccountData.Username, false);
            await LoadFriendsAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lvFriends_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // Giữ nguyên width do mình set trong SetupColumns (khỏi bị resize lệch icon)
            e.Cancel = true;
            e.NewWidth = lvFriends.Columns[e.ColumnIndex].Width;
        }
    }
}
