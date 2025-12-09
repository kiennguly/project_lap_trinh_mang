using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
           
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            // --- Placeholder cho Username ---
            textBoxUser.Text = "Tên đăng nhập";
            textBoxUser.ForeColor = Color.Gray;

            // --- Placeholder cho Password ---
            textBoxPass.Text = "Mật khẩu";
            textBoxPass.ForeColor = Color.Gray;
            textBoxPass.UseSystemPasswordChar = false;

            // --- Khi nhập username ---
            textBoxUser.Enter += (s, ev) =>
            {
                if (textBoxUser.Text == "Tên đăng nhập")
                {
                    textBoxUser.Text = "";
                    textBoxUser.ForeColor = Color.Black;
                }
            };
            textBoxUser.Leave += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(textBoxUser.Text))
                {
                    textBoxUser.Text = "Tên đăng nhập";
                    textBoxUser.ForeColor = Color.Gray;
                }
            };

            // --- Khi nhập password ---
            textBoxPass.Enter += (s, ev) =>
            {
                if (textBoxPass.Text == "Mật khẩu")
                {
                    textBoxPass.Text = "";
                    textBoxPass.ForeColor = Color.Black;
                }
                textBoxPass.UseSystemPasswordChar = !checkBoxShow.Checked;
            };
            textBoxPass.Leave += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(textBoxPass.Text))
                {
                    textBoxPass.Text = "Mật khẩu";
                    textBoxPass.ForeColor = Color.Gray;
                    textBoxPass.UseSystemPasswordChar = false;
                }
            };

            // --- Hiện/ẩn mật khẩu ---
            checkBoxShow.CheckedChanged += (s, ev) =>
            {
                if (textBoxPass.Text != "Mật khẩu")
                    textBoxPass.UseSystemPasswordChar = !checkBoxShow.Checked;
            };
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            
            string username = textBoxUser.Text.Trim();
            string password = textBoxPass.Text.Trim();

            if (username == "Tên đăng nhập" || password == "Mật khẩu" ||
                username == "" || password == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool loginSuccess = Database.CheckLogin(username, password);

            if (loginSuccess)
            {
                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 1) Tạo form Menu
                Menu form3 = new Menu();

                // 2) Khi Menu đóng -> hiện lại Login
                form3.FormClosed += (s, args) =>
                {
                    this.Show();   // hiện lại form Login
                };

                // 3) Ẩn Login và show Menu
                this.Hide();
                form3.Show();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }


        private void buttonRegister_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (Register reg = new Register())
            {
                reg.ShowDialog();
            }

            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (var reset = new FogotPass())
            {
                reset.StartPosition = FormStartPosition.CenterParent;
                reset.ShowDialog(this); 
            }
            this.Show();
        }

        private void textBoxUser_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
    