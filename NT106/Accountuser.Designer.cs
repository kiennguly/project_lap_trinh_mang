using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class Accountuser
    {
        private System.ComponentModel.IContainer components = null;

        private PictureBox pictureAvatar;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblEmail;
        private Label lblLevel;
        private Label lblRank;
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtLevel;
        private TextBox txtRank;
        private Button btnClose;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pictureAvatar = new PictureBox();
            lblTitle = new Label();
            lblUsername = new Label();
            lblEmail = new Label();
            lblLevel = new Label();
            lblRank = new Label();
            txtUsername = new TextBox();
            txtEmail = new TextBox();
            txtLevel = new TextBox();
            txtRank = new TextBox();
            btnClose = new Button();
            button3 = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureAvatar).BeginInit();
            SuspendLayout();
            // 
            // pictureAvatar
            // 
            pictureAvatar.BackColor = Color.FromArgb(20, 25, 50);
            pictureAvatar.BorderStyle = BorderStyle.FixedSingle;
            pictureAvatar.Location = new Point(30, 70);
            pictureAvatar.Name = "pictureAvatar";
            pictureAvatar.Size = new Size(150, 150);
            pictureAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pictureAvatar.TabIndex = 0;
            pictureAvatar.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(90, 0, 0, 0);
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 255, 255);
            lblTitle.Location = new Point(0, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(600, 40);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "THÔNG TIN TÀI KHOẢN";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.BackColor = Color.FromArgb(90, 0, 0, 0);
            lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(0, 192, 192);
            lblUsername.Location = new Point(210, 80);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(89, 23);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username";
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.BackColor = Color.FromArgb(90, 0, 0, 0);
            lblEmail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEmail.ForeColor = Color.FromArgb(0, 192, 192);
            lblEmail.Location = new Point(210, 130);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(54, 23);
            lblEmail.TabIndex = 3;
            lblEmail.Text = "Email";
            // 
            // lblLevel
            // 
            lblLevel.AutoSize = true;
            lblLevel.BackColor = Color.FromArgb(90, 0, 0, 0);
            lblLevel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLevel.ForeColor = Color.FromArgb(0, 192, 192);
            lblLevel.Location = new Point(210, 180);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(51, 23);
            lblLevel.TabIndex = 4;
            lblLevel.Text = "Level";
            // 
            // lblRank
            // 
            lblRank.AutoSize = true;
            lblRank.BackColor = Color.FromArgb(90, 0, 0, 0);
            lblRank.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRank.ForeColor = Color.FromArgb(0, 192, 192);
            lblRank.Location = new Point(210, 230);
            lblRank.Name = "lblRank";
            lblRank.Size = new Size(50, 23);
            lblRank.TabIndex = 5;
            lblRank.Text = "Rank";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(15, 22, 45);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtUsername.ForeColor = Color.FromArgb(0, 255, 255);
            txtUsername.Location = new Point(320, 76);
            txtUsername.Name = "txtUsername";
            txtUsername.ReadOnly = true;
            txtUsername.Size = new Size(240, 30);
            txtUsername.TabIndex = 6;
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.FromArgb(15, 22, 45);
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtEmail.ForeColor = Color.FromArgb(0, 255, 255);
            txtEmail.Location = new Point(320, 126);
            txtEmail.Name = "txtEmail";
            txtEmail.ReadOnly = true;
            txtEmail.Size = new Size(240, 30);
            txtEmail.TabIndex = 7;
            // 
            // txtLevel
            // 
            txtLevel.BackColor = Color.FromArgb(15, 22, 45);
            txtLevel.BorderStyle = BorderStyle.FixedSingle;
            txtLevel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtLevel.ForeColor = Color.FromArgb(0, 255, 255);
            txtLevel.Location = new Point(320, 176);
            txtLevel.Name = "txtLevel";
            txtLevel.ReadOnly = true;
            txtLevel.Size = new Size(240, 30);
            txtLevel.TabIndex = 8;
            // 
            // txtRank
            // 
            txtRank.BackColor = Color.FromArgb(15, 22, 45);
            txtRank.BorderStyle = BorderStyle.FixedSingle;
            txtRank.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtRank.ForeColor = Color.FromArgb(0, 255, 255);
            txtRank.Location = new Point(320, 226);
            txtRank.Name = "txtRank";
            txtRank.ReadOnly = true;
            txtRank.Size = new Size(240, 30);
            txtRank.TabIndex = 9;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(15, 25, 45);
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(0, 192, 192);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(0, 192, 192);
            btnClose.Location = new Point(230, 295);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(140, 40);
            btnClose.TabIndex = 10;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // button3
            // 
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            button3.ForeColor = Color.FromArgb(0, 192, 192);
            button3.Location = new Point(30, 297);
            button3.Name = "button3";
            button3.Size = new Size(160, 38);
            button3.TabIndex = 11;
            button3.Text = "Đổi Pass";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button1
            // 
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            button1.ForeColor = Color.FromArgb(0, 192, 192);
            button1.Location = new Point(412, 295);
            button1.Name = "button1";
            button1.Size = new Size(160, 38);
            button1.TabIndex = 12;
            button1.Text = "GiftCode";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Accountuser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 30);
            BackgroundImage = Properties.Resource.anh_form_account_user;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(600, 380);
            Controls.Add(button1);
            Controls.Add(button3);
            Controls.Add(btnClose);
            Controls.Add(txtRank);
            Controls.Add(txtLevel);
            Controls.Add(txtEmail);
            Controls.Add(txtUsername);
            Controls.Add(lblRank);
            Controls.Add(lblLevel);
            Controls.Add(lblEmail);
            Controls.Add(lblUsername);
            Controls.Add(lblTitle);
            Controls.Add(pictureAvatar);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Accountuser";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Thông tin tài khoản";
            Load += Accountuser_Load;
            ((System.ComponentModel.ISupportInitialize)pictureAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button3;
        private Button button1;
    }
}
