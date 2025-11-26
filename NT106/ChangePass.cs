using System;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class ChangePass : Form
    {
        public ChangePass()
        {
            InitializeComponent();
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AccountData.Username))
            {
                MessageBox.Show("Bạn cần đăng nhập trước khi đổi mật khẩu!");
                return;
            }

            string newPass = textBoxNewPass.Text.Trim();

            if (string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!");
                textBoxNewPass.Focus();
                return;
            }

            if (newPass.Length < 4)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 4 ký tự!");
                textBoxNewPass.Focus();
                return;
            }

            bool success = Database.ChangePassword(AccountData.Username, newPass);

            if (success)
            {
                AccountData.Password = newPass; // cập nhật lại local
                MessageBox.Show("Đổi mật khẩu thành công!");
                this.Close();
            }
            else
            {
                // Thông báo lỗi chi tiết đã hiển thị trong Database.ChangePassword
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ChangePass_Load(object sender, EventArgs e)
        {

        }
    }
}
