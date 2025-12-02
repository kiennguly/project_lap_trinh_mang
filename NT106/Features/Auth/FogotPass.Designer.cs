using System.Windows.Forms;
using System.Drawing;

namespace plan_fighting_super_start
{
    partial class FogotPass
    {
        private System.ComponentModel.IContainer components = null;

        private Label labelTitle;
        private Label labelUser;
        private Label labelEmail;
        private Label labelCode;
        private Label labelNewPass;
        private Label labelConfirmPass;

        private TextBox textBoxUser;
        private TextBox textBoxEmail;
        private TextBox textBoxCode;
        private TextBox textBoxNewPass;
        private TextBox textBoxConfirmPass;

        private Button buttonSendCode;
        private Button buttonConfirm;
        private Button buttonCancel;

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
            labelTitle = new Label();
            labelUser = new Label();
            labelEmail = new Label();
            labelCode = new Label();
            labelNewPass = new Label();
            labelConfirmPass = new Label();
            textBoxUser = new TextBox();
            textBoxEmail = new TextBox();
            textBoxCode = new TextBox();
            textBoxNewPass = new TextBox();
            textBoxConfirmPass = new TextBox();
            buttonSendCode = new Button();
            buttonConfirm = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.BackColor = Color.Transparent;
            labelTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelTitle.ForeColor = Color.FromArgb(0, 192, 192);
            labelTitle.Location = new Point(110, 9);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(272, 41);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "QUÊN MẬT KHẨU";
            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            labelUser.BackColor = Color.Transparent;
            labelUser.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            labelUser.ForeColor = Color.FromArgb(0, 192, 192);
            labelUser.Location = new Point(70, 64);
            labelUser.Name = "labelUser";
            labelUser.Size = new Size(128, 23);
            labelUser.TabIndex = 1;
            labelUser.Text = "Tên đăng nhập";
            // 
            // labelEmail
            // 
            labelEmail.AutoSize = true;
            labelEmail.BackColor = Color.Transparent;
            labelEmail.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            labelEmail.ForeColor = Color.FromArgb(0, 192, 192);
            labelEmail.Location = new Point(70, 129);
            labelEmail.Name = "labelEmail";
            labelEmail.Size = new Size(57, 23);
            labelEmail.TabIndex = 3;
            labelEmail.Text = "Gmail";
            // 
            // labelCode
            // 
            labelCode.AutoSize = true;
            labelCode.BackColor = Color.Transparent;
            labelCode.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            labelCode.ForeColor = Color.FromArgb(0, 192, 192);
            labelCode.Location = new Point(70, 249);
            labelCode.Name = "labelCode";
            labelCode.Size = new Size(112, 23);
            labelCode.TabIndex = 6;
            labelCode.Text = "Mã xác minh";
            // 
            // labelNewPass
            // 
            labelNewPass.AutoSize = true;
            labelNewPass.BackColor = Color.Transparent;
            labelNewPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            labelNewPass.ForeColor = Color.FromArgb(0, 192, 192);
            labelNewPass.Location = new Point(70, 314);
            labelNewPass.Name = "labelNewPass";
            labelNewPass.Size = new Size(123, 23);
            labelNewPass.TabIndex = 8;
            labelNewPass.Text = "Mật khẩu mới";
            // 
            // labelConfirmPass
            // 
            labelConfirmPass.AutoSize = true;
            labelConfirmPass.BackColor = Color.Transparent;
            labelConfirmPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            labelConfirmPass.ForeColor = Color.FromArgb(0, 192, 192);
            labelConfirmPass.Location = new Point(70, 379);
            labelConfirmPass.Name = "labelConfirmPass";
            labelConfirmPass.Size = new Size(158, 23);
            labelConfirmPass.TabIndex = 10;
            labelConfirmPass.Text = "Nhập lại mật khẩu";
            // 
            // textBoxUser
            // 
            textBoxUser.BackColor = Color.FromArgb(15, 25, 45);
            textBoxUser.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            textBoxUser.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxUser.Location = new Point(70, 89);
            textBoxUser.Name = "textBoxUser";
            textBoxUser.PlaceholderText = "Nhập tên đăng nhập";
            textBoxUser.Size = new Size(340, 30);
            textBoxUser.TabIndex = 0;
            // 
            // textBoxEmail
            // 
            textBoxEmail.BackColor = Color.FromArgb(15, 25, 45);
            textBoxEmail.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            textBoxEmail.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxEmail.Location = new Point(70, 154);
            textBoxEmail.Name = "textBoxEmail";
            textBoxEmail.PlaceholderText = "Nhập Gmail đã đăng ký";
            textBoxEmail.Size = new Size(340, 30);
            textBoxEmail.TabIndex = 1;
            // 
            // textBoxCode
            // 
            textBoxCode.BackColor = Color.FromArgb(15, 25, 45);
            textBoxCode.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            textBoxCode.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxCode.Location = new Point(70, 274);
            textBoxCode.Name = "textBoxCode";
            textBoxCode.PlaceholderText = "Nhập mã đã gửi qua Gmail";
            textBoxCode.Size = new Size(340, 30);
            textBoxCode.TabIndex = 3;
            // 
            // textBoxNewPass
            // 
            textBoxNewPass.BackColor = Color.FromArgb(15, 25, 45);
            textBoxNewPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            textBoxNewPass.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxNewPass.Location = new Point(70, 339);
            textBoxNewPass.Name = "textBoxNewPass";
            textBoxNewPass.PlaceholderText = "Nhập mật khẩu mới";
            textBoxNewPass.Size = new Size(340, 30);
            textBoxNewPass.TabIndex = 4;
            textBoxNewPass.UseSystemPasswordChar = true;
            // 
            // textBoxConfirmPass
            // 
            textBoxConfirmPass.BackColor = Color.FromArgb(15, 25, 45);
            textBoxConfirmPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            textBoxConfirmPass.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxConfirmPass.Location = new Point(70, 404);
            textBoxConfirmPass.Name = "textBoxConfirmPass";
            textBoxConfirmPass.PlaceholderText = "Nhập lại mật khẩu mới";
            textBoxConfirmPass.Size = new Size(340, 30);
            textBoxConfirmPass.TabIndex = 5;
            textBoxConfirmPass.UseSystemPasswordChar = true;
            // 
            // buttonSendCode
            // 
            buttonSendCode.BackColor = Color.FromArgb(15, 25, 45);
            buttonSendCode.FlatStyle = FlatStyle.Flat;
            buttonSendCode.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonSendCode.ForeColor = Color.FromArgb(0, 192, 192);
            buttonSendCode.Location = new Point(70, 199);
            buttonSendCode.Name = "buttonSendCode";
            buttonSendCode.Size = new Size(155, 35);
            buttonSendCode.TabIndex = 2;
            buttonSendCode.Text = "Gửi mã";
            buttonSendCode.UseVisualStyleBackColor = false;
            buttonSendCode.Click += buttonSendCode_Click;
            // 
            // buttonConfirm
            // 
            buttonConfirm.BackColor = Color.FromArgb(15, 25, 45);
            buttonConfirm.FlatStyle = FlatStyle.Flat;
            buttonConfirm.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonConfirm.ForeColor = Color.FromArgb(0, 192, 192);
            buttonConfirm.Location = new Point(70, 449);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(155, 35);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Đổi mật khẩu";
            buttonConfirm.UseVisualStyleBackColor = false;
            buttonConfirm.Click += buttonConfirm_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.BackColor = Color.Transparent;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonCancel.ForeColor = Color.FromArgb(0, 192, 192);
            buttonCancel.Location = new Point(255, 449);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(155, 35);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Hủy";
            buttonCancel.UseVisualStyleBackColor = false;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // FogotPass
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImage = Properties.Resource.Gemini_Generated_Image_47v10s47v10s47v1;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(480, 515);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBoxConfirmPass);
            Controls.Add(labelConfirmPass);
            Controls.Add(textBoxNewPass);
            Controls.Add(labelNewPass);
            Controls.Add(textBoxCode);
            Controls.Add(labelCode);
            Controls.Add(buttonSendCode);
            Controls.Add(textBoxEmail);
            Controls.Add(labelEmail);
            Controls.Add(textBoxUser);
            Controls.Add(labelUser);
            Controls.Add(labelTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FogotPass";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quên mật khẩu";
            Load += FogotPass_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
