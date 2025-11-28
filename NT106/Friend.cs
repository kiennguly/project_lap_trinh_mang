using System;
using System.Collections.Generic;
using System.ComponentModel;       // 👈 thêm namespace này
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Friend : Form
    {
        // ImageList vẫn tạo bình thường, không sao với Designer
        private readonly ImageList _avatarImageList = new ImageList();

        // ⚠ KHÔNG khởi tạo trực tiếp nữa, để null rồi tạo lúc chạy thật
        private S3ImageService? _imageService;

        private List<FriendEntry> _friends = new List<FriendEntry>();
        private bool _isLoading = false;

        public Friend()
        {
            InitializeComponent();

            // 🔒 Nếu đang mở trong Designer thì dừng tại đây,
            // không làm gì thêm để tránh tạo AWS client / gọi DB.
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _avatarImageList.ImageSize = new Size(48, 48);
            _avatarImageList.ColorDepth = ColorDepth.Depth32Bit;
            lvFriends.SmallImageList = _avatarImageList;

            // chỉ tạo S3ImageService khi chạy app thật
            _imageService = new S3ImageService();

            StyleButtons();
        }

        private void StyleButtons()
        {
            Color btnBg = Color.FromArgb(20, 40, 70);
            Color btnBorder = Color.Cyan;
            Button[] buttons = { btnSendRequest, btnAccept, btnDecline, btnRefresh };

            foreach (var ctrl in buttons)
            {
                ctrl.BackColor = btnBg;
                ctrl.ForeColor = Color.Cyan;
                ctrl.FlatStyle = FlatStyle.Flat;
                ctrl.FlatAppearance.BorderColor = btnBorder;
                ctrl.FlatAppearance.BorderSize = 1;
            }
        }

        private async void Friend_Load(object sender, EventArgs e)
        {
            // thêm bảo hiểm lần nữa
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            await LoadFriendsAsync();
        }

        // Khóa width các cột, không cho kéo
        private void lvFriends_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = lvFriends.Columns[e.ColumnIndex].Width;
        }

        private async Task LoadFriendsAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            btnRefresh.Enabled = false;
            btnAccept.Enabled = false;
            btnDecline.Enabled = false;
            lblLoading.Visible = true;

            _avatarImageList.Images.Clear();
            lvFriends.Items.Clear();

            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập trước khi xem bạn bè.");
                _isLoading = false;
                btnRefresh.Enabled = true;
                lblLoading.Visible = false;
                return;
            }

            var startTime = DateTime.UtcNow;

            try
            {
                _friends = await Database.GetFriendListAsync(AccountData.Username);

                lvFriends.BeginUpdate();
                foreach (var f in _friends)
                {
                    int imgIndex = await DownloadAvatarByUsernameAsync(f.Username);

                    var item = new ListViewItem("");
                    if (imgIndex >= 0)
                        item.ImageIndex = imgIndex;

                    item.SubItems.Add(f.Username);
                    item.SubItems.Add(StatusToVietnamese(f.Status));
                    item.Tag = f;

                    lvFriends.Items.Add(item);
                }
                lvFriends.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bạn bè: " + ex.Message,
                    "Friend", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                var elapsed = DateTime.UtcNow - startTime;
                if (elapsed.TotalMilliseconds < 250)
                    await Task.Delay(250 - (int)elapsed.TotalMilliseconds);

                _isLoading = false;
                btnRefresh.Enabled = true;
                btnAccept.Enabled = false;
                btnDecline.Enabled = false;
                lblLoading.Visible = false;
            }
        }

        // Lấy avatar giống như ở Menu: avatars/{username}.png
        private async Task<int> DownloadAvatarByUsernameAsync(string username)
        {
            try
            {
                // đảm bảo _imageService đã được tạo
                if (_imageService == null)
                    _imageService = new S3ImageService();

                string key = $"avatars/avatars/{username}.png";
                var img = await _imageService.GetImageAsync(key);
                if (img == null) return -1;

                _avatarImageList.Images.Add(img);
                return _avatarImageList.Images.Count - 1;
            }
            catch
            {
                return -1;
            }
        }

        private string StatusToVietnamese(string status)
        {
            switch (status)
            {
                case "pending":
                    return "Đang chờ bạn chấp nhận";
                case "sent":
                    return "Đã gửi lời mời";
                case "accepted":
                    return "Bạn bè";
                default:
                    return status;
            }
        }

        private async void btnSendRequest_Click(object sender, EventArgs e)
        {
            var target = txtFriendUsername.Text.Trim();

            if (string.IsNullOrEmpty(target))
            {
                MessageBox.Show("Vui lòng nhập username bạn bè.");
                return;
            }

            if (string.Equals(target, AccountData.Username, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể kết bạn với chính mình.");
                return;
            }

            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập trước.");
                return;
            }

            btnSendRequest.Enabled = false;

            try
            {
                bool exists = await Database.CheckAccountExistsAsync(target);
                if (!exists)
                {
                    MessageBox.Show(
                        $"Tài khoản \"{target}\" không tồn tại.",
                        "Friend",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                var ok = await Database.SendFriendRequestAsync(AccountData.Username, target);
                if (ok)
                {
                    await LoadFriendsAsync();
                    txtFriendUsername.Clear();
                }
            }
            finally
            {
                btnSendRequest.Enabled = true;
            }
        }

        private void lvFriends_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvFriends.SelectedItems.Count == 0)
            {
                btnAccept.Enabled = false;
                btnDecline.Enabled = false;
                return;
            }

            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            if (entry == null)
            {
                btnAccept.Enabled = false;
                btnDecline.Enabled = false;
                return;
            }

            bool canRespond = string.Equals(entry.Status, "pending", StringComparison.OrdinalIgnoreCase);
            btnAccept.Enabled = canRespond;
            btnDecline.Enabled = canRespond;
        }

        private async void btnAccept_Click(object sender, EventArgs e)
        {
            if (lvFriends.SelectedItems.Count == 0) return;

            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            if (entry == null) return;

            btnAccept.Enabled = false;
            btnDecline.Enabled = false;

            await Database.RespondFriendRequestAsync(entry.Username, AccountData.Username, true);
            await LoadFriendsAsync();
        }

        private async void btnDecline_Click(object sender, EventArgs e)
        {
            if (lvFriends.SelectedItems.Count == 0) return;

            var entry = lvFriends.SelectedItems[0].Tag as FriendEntry;
            if (entry == null) return;

            btnAccept.Enabled = false;
            btnDecline.Enabled = false;

            await Database.RespondFriendRequestAsync(entry.Username, AccountData.Username, false);
            await LoadFriendsAsync();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadFriendsAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
