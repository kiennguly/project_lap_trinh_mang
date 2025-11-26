using System;
using System.Drawing;
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
            // Placeholder cho 3 textbox
            if (string.IsNullOrWhiteSpace(textBoxUser.Text) ||
                textBoxUser.Text == "Tên đăng nhập")
            {
                textBoxUser.Text = "Tên đăng nhập";
                textBoxUser.ForeColor = Color.Gray;
            }

            if (string.IsNullOrWhiteSpace(textBoxEmail.Text) ||
                textBoxEmail.Text == "Gmail")
            {
                textBoxEmail.Text = "Gmail";
                textBoxEmail.ForeColor = Color.Gray;
            }

            if (string.IsNullOrWhiteSpace(textBoxPass.Text) ||
                textBoxPass.Text == "Mật khẩu")
            {
                textBoxPass.Text = "Mật khẩu";
                textBoxPass.ForeColor = Color.Gray;
                textBoxPass.UseSystemPasswordChar = false;
            }

            // Có thể set avatar default nếu muốn
            // pictureBoxAvatar.Image = Properties.Resource.DefaultAvatar;
            // pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        }

        // NÚT ĐĂNG KÝ – có async vì cần await upload ảnh
        private async void buttonRegister_Click(object sender, EventArgs e)
        {
            string user = textBoxUser.Text.Trim();
            string pass = textBoxPass.Text.Trim();
            string email = textBoxEmail.Text.Trim();

            // Kiểm tra nhập đầy đủ
            if (string.IsNullOrWhiteSpace(user) || user == "Tên đăng nhập" ||
                string.IsNullOrWhiteSpace(pass) || pass == "Mật khẩu" ||
                string.IsNullOrWhiteSpace(email) || email == "Gmail")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập, Mật khẩu và Gmail!");
                return;
            }

            // Check format email đơn giản
            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Gmail không hợp lệ, vui lòng kiểm tra lại!");
                return;
            }

            // 🔥 BẮT BUỘC PHẢI CHỌN ẢNH AVATAR
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
            catch (Exception ex)
            {
                MessageBox.Show("Upload ảnh avatar thất bại: " + ex.Message);
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

        // Ẩn password khi bắt đầu gõ
        private void textBoxPass_TextChanged(object sender, EventArgs e)
        {
            if (textBoxPass.ForeColor == Color.Gray && textBoxPass.Text == "Mật khẩu")
            {
                textBoxPass.Text = "";
                textBoxPass.ForeColor = Color.FromArgb(0, 192, 192);
                textBoxPass.UseSystemPasswordChar = true;
            }
        }

        // Nếu bạn muốn pictureBox1 là nút đóng form / quay lại
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // this.Close();
            // hoặc this.Hide();
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
