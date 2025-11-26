using System.Windows.Forms;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using System.Drawing;

namespace plan_fighting_super_start
{
    partial class Register
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox textBoxUser;
        private TextBox textBoxPass;
        private TextBox textBoxEmail;
        private Button buttonRegister;

        private PictureBox pictureBox2;
        private Label label1;
        private Label label3;
        private Label label2;
        private Label label4;
        private PictureBox pictureBox1;
        private PictureBox pictureBox3;

        // 🔹 MỚI
        private PictureBox pictureBoxAvatar;
        private Button buttonChooseAvatar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            textBoxUser = new TextBox();
            textBoxPass = new TextBox();
            textBoxEmail = new TextBox();
            buttonRegister = new Button();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            label3 = new Label();
            label2 = new Label();
            label4 = new Label();
            pictureBox1 = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBoxAvatar = new PictureBox();
            buttonChooseAvatar = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAvatar).BeginInit();
            SuspendLayout();
            // 
            // textBoxUser
            // 
            textBoxUser.BackColor = Color.White;
            textBoxUser.Font = new Font("Segoe UI", 10.2F);
            textBoxUser.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxUser.Location = new Point(127, 240);
            textBoxUser.Name = "textBoxUser";
            textBoxUser.PlaceholderText = "Tên đăng nhập";
            textBoxUser.Size = new Size(200, 30);
            textBoxUser.TabIndex = 0;
            textBoxUser.Text = "Tên đăng nhập";
            // 
            // textBoxPass
            // 
            textBoxPass.BackColor = Color.White;
            textBoxPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 163);
            textBoxPass.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxPass.Location = new Point(127, 320);
            textBoxPass.Name = "textBoxPass";
            textBoxPass.PlaceholderText = "Mật khẩu";
            textBoxPass.Size = new Size(200, 30);
            textBoxPass.TabIndex = 2;
            textBoxPass.Text = "Mật khẩu";
            textBoxPass.UseSystemPasswordChar = true;
            textBoxPass.TextChanged += textBoxPass_TextChanged;
            // 
            // textBoxEmail
            // 
            textBoxEmail.BackColor = Color.White;
            textBoxEmail.Font = new Font("Segoe UI", 10.2F);
            textBoxEmail.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxEmail.Location = new Point(127, 280);
            textBoxEmail.Name = "textBoxEmail";
            textBoxEmail.PlaceholderText = "Gmail";
            textBoxEmail.Size = new Size(200, 30);
            textBoxEmail.TabIndex = 1;
            textBoxEmail.Text = "Gmail";
            // 
            // buttonRegister
            // 
            buttonRegister.BackColor = Color.Transparent;
            buttonRegister.FlatStyle = FlatStyle.Flat;
            buttonRegister.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonRegister.ForeColor = Color.FromArgb(0, 192, 192);
            buttonRegister.Location = new Point(170, 370);
            buttonRegister.Name = "buttonRegister";
            buttonRegister.Size = new Size(90, 36);
            buttonRegister.TabIndex = 3;
            buttonRegister.Text = "Đăng ký";
            buttonRegister.UseVisualStyleBackColor = false;
            buttonRegister.Click += buttonRegister_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.BackgroundImage = Properties.Resource.Screenshot_2025_09_19_232602;
            pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox2.Location = new Point(155, 129);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(144, 87);
            pictureBox2.TabIndex = 7;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.ForeColor = Color.FromArgb(0, 192, 192);
            label1.Location = new Point(155, 65);
            label1.Name = "label1";
            label1.Size = new Size(203, 54);
            label1.TabIndex = 10;
            label1.Text = "REGISTER";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.White;
            label3.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(0, 192, 192);
            label3.Location = new Point(89, 324);
            label3.Name = "label3";
            label3.Size = new Size(34, 23);
            label3.TabIndex = 12;
            label3.Text = "🔒";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.White;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(0, 192, 192);
            label2.Location = new Point(89, 244);
            label2.Name = "label2";
            label2.Size = new Size(34, 23);
            label2.TabIndex = 11;
            label2.Text = "👤";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.White;
            label4.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(0, 192, 192);
            label4.Location = new Point(89, 284);
            label4.Name = "label4";
            label4.Size = new Size(34, 23);
            label4.TabIndex = 13;
            label4.Text = "✉️";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.BackgroundImage = Properties.Resource.Screenshot_2025_09_21_175525;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.Location = new Point(300, -25);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(157, 102);
            pictureBox1.TabIndex = 14;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.BackgroundImage = Properties.Resource.Screenshot_2025_09_21_180231;
            pictureBox3.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox3.Location = new Point(-46, 364);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(193, 100);
            pictureBox3.TabIndex = 15;
            pictureBox3.TabStop = false;
            // 
            // pictureBoxAvatar
            // 
            pictureBoxAvatar.BackColor = Color.White;
            pictureBoxAvatar.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxAvatar.Location = new Point(11, 12);
            pictureBoxAvatar.Name = "pictureBoxAvatar";
            pictureBoxAvatar.Size = new Size(80, 80);
            pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAvatar.TabIndex = 16;
            pictureBoxAvatar.TabStop = false;
            // 
            // buttonChooseAvatar
            // 
            buttonChooseAvatar.BackColor = Color.Transparent;
            buttonChooseAvatar.FlatStyle = FlatStyle.Flat;
            buttonChooseAvatar.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonChooseAvatar.ForeColor = Color.FromArgb(0, 192, 192);
            buttonChooseAvatar.Location = new Point(11, 107);
            buttonChooseAvatar.Name = "buttonChooseAvatar";
            buttonChooseAvatar.Size = new Size(100, 28);
            buttonChooseAvatar.TabIndex = 4;
            buttonChooseAvatar.Text = "Chọn avatar";
            buttonChooseAvatar.UseVisualStyleBackColor = false;
            buttonChooseAvatar.Click += buttonChooseAvatar_Click;
            // 
            // Register
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(439, 441);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox1);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox2);
            Controls.Add(textBoxUser);
            Controls.Add(textBoxEmail);
            Controls.Add(textBoxPass);
            Controls.Add(buttonRegister);
            Controls.Add(pictureBoxAvatar);
            Controls.Add(buttonChooseAvatar);
            Name = "Register";
            Text = "Đăng ký";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
