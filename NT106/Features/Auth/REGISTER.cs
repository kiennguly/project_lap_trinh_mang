using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Register : Form
    {
        // Dịch vụ làm việc với S3
        private readonly S3ImageService _imageService = new S3ImageService();

        // Đường dẫn ảnh avatar mà user chọn trên máy
        private string _avatarFilePath = null;

        public Register()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // --- User ---
            if (string.IsNullOrWhiteSpace(textBoxUser.Text))
            {
                textBoxUser.Text = "Tên đăng nhập";
            }
            textBoxUser.ForeColor = Color.Gray;

            // --- Email ---
            if (string.IsNullOrWhiteSpace(textBoxEmail.Text))
            {
                textBoxEmail.Text = "Gmail";
            }
            textBoxEmail.ForeColor = Color.Gray;

            // --- Password ---
            if (string.IsNullOrWhiteSpace(textBoxPass.Text))
            {
                textBoxPass.Text = "Mật khẩu";
            }
            textBoxPass.ForeColor = Color.Gray;
            textBoxPass.UseSystemPasswordChar = false;
        }

        // NÚT ĐĂNG KÝ – có async vì cần await upload ảnh
        private async void buttonRegister_Click(object sender, EventArgs e)
        {
            string user = textBoxUser.Text.Trim();
            string pass = textBoxPass.Text.Trim();
            string email = textBoxEmail.Text.Trim();

            // Bỏ placeholder nếu user chưa sửa
            if (user == "Tên đăng nhập") user = "";
            if (email == "Gmail") email = "";
            if (pass == "Mật khẩu") pass = "";

            // Kiểm tra nhập đầy đủ
            if (string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập, Mật khẩu và Email!");
                return;
            }

            // Check format email (cho cả Gmail cá nhân + Gmail doanh nghiệp)
            if (!Regex.IsMatch(email,
                    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Email không hợp lệ, vui lòng kiểm tra lại!");
                return;
            }

            // BẮT BUỘC PHẢI CHỌN ẢNH AVATAR
            if (string.IsNullOrEmpty(_avatarFilePath))
            {
                MessageBox.Show("Vui lòng chọn ảnh avatar trước khi đăng ký!");
                return;
            }

            // Upload avatar lên S3 với tên avatars/{username}.png
            try
            {
                await _imageService.UploadImageAsync(_avatarFilePath, user);
            }
            catch (Exception)
            {
                MessageBox.Show("Upload ảnh avatar thất bại, vui lòng thử lại!");
                return;
            }

            // Gọi lại hàm đăng ký cũ – 3 tham số như trước
            bool success = Database.RegisterAccount(user, pass, email);

            if (success)
            {
                MessageBox.Show("Đăng ký thành công!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại hoặc Email bị trùng!");
            }
        }

        // Ẩn password / placeholder khi focus
        private void textBoxPass_Enter(object sender, EventArgs e)
        {
            if (textBoxPass.Text == "Mật khẩu")
            {
                textBoxPass.Text = "";
                textBoxPass.ForeColor = Color.FromArgb(0, 192, 192);
                textBoxPass.UseSystemPasswordChar = true;
            }
        }

        private void textBoxPass_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxPass.Text))
            {
                textBoxPass.UseSystemPasswordChar = false;
                textBoxPass.Text = "Mật khẩu";
                textBoxPass.ForeColor = Color.Gray;
            }
        }

        // Placeholder cho User
        private void textBoxUser_Enter(object sender, EventArgs e)
        {
            if (textBoxUser.Text == "Tên đăng nhập")
            {
                textBoxUser.Text = "";
                textBoxUser.ForeColor = Color.FromArgb(0, 192, 192);
            }
        }

        private void textBoxUser_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxUser.Text))
            {
                textBoxUser.Text = "Tên đăng nhập";
                textBoxUser.ForeColor = Color.Gray;
            }
        }

        // Placeholder cho Email
        private void textBoxEmail_Enter(object sender, EventArgs e)
        {
            if (textBoxEmail.Text == "Gmail")
            {
                textBoxEmail.Text = "";
                textBoxEmail.ForeColor = Color.FromArgb(0, 192, 192);
            }
        }

        private void textBoxEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxEmail.Text))
            {
                textBoxEmail.Text = "Gmail";
                textBoxEmail.ForeColor = Color.Gray;
            }
        }

        // Nếu muốn pictureBox1 là nút đóng form / quay lại
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // NÚT CHỌN ẢNH AVATAR
        private void buttonChooseAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh avatar";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _avatarFilePath = ofd.FileName;

                    try
                    {
                        var img = Image.FromFile(_avatarFilePath);
                        pictureBoxAvatar.Image = img;
                        pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể load ảnh: " + ex.Message);
                        _avatarFilePath = null;
                    }
                }
            }
        }
    }
}
