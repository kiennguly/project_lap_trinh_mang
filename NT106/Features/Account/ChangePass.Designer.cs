using System.Windows.Forms;
using System.Drawing;

namespace plan_fighting_super_start
{
    partial class ChangePass
    {
        private System.ComponentModel.IContainer components = null;

        private Label labelTitle;
        private Label labelNewPass;
        private TextBox textBoxNewPass;
        private Button buttonChange;
        private Button buttonCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            labelTitle = new Label();
            labelNewPass = new Label();
            textBoxNewPass = new TextBox();
            buttonChange = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.BackColor = Color.Transparent;
            labelTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelTitle.ForeColor = Color.FromArgb(0, 192, 192);
            labelTitle.Location = new Point(53, 12);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(194, 32);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "ĐỔI MẬT KHẨU";
            // 
            // labelNewPass
            // 
            labelNewPass.AutoSize = true;
            labelNewPass.BackColor = Color.Transparent;
            labelNewPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            labelNewPass.ForeColor = Color.FromArgb(0, 192, 192);
            labelNewPass.Location = new Point(29, 84);
            labelNewPass.Name = "labelNewPass";
            labelNewPass.Size = new Size(123, 23);
            labelNewPass.TabIndex = 1;
            labelNewPass.Text = "Mật khẩu mới";
            // 
            // textBoxNewPass
            // 
            textBoxNewPass.BackColor = Color.FromArgb(15, 25, 45);
            textBoxNewPass.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 163);
            textBoxNewPass.ForeColor = Color.FromArgb(0, 192, 192);
            textBoxNewPass.Location = new Point(29, 120);
            textBoxNewPass.Name = "textBoxNewPass";
            textBoxNewPass.PlaceholderText = "Nhập mật khẩu mới";
            textBoxNewPass.Size = new Size(260, 30);
            textBoxNewPass.TabIndex = 0;
            textBoxNewPass.UseSystemPasswordChar = true;
            // 
            // buttonChange
            // 
            buttonChange.BackColor = Color.FromArgb(15, 25, 45);
            buttonChange.FlatStyle = FlatStyle.Flat;
            buttonChange.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonChange.ForeColor = Color.FromArgb(0, 192, 192);
            buttonChange.Location = new Point(29, 170);
            buttonChange.Name = "buttonChange";
            buttonChange.Size = new Size(120, 35);
            buttonChange.TabIndex = 1;
            buttonChange.Text = "Đổi mật khẩu";
            buttonChange.UseVisualStyleBackColor = false;
            buttonChange.Click += buttonChange_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.BackColor = Color.FromArgb(15, 25, 45);
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonCancel.ForeColor = Color.FromArgb(0, 192, 192);
            buttonCancel.Location = new Point(169, 170);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(120, 35);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Hủy";
            buttonCancel.UseVisualStyleBackColor = false;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // ChangePass
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImage = Properties.Resource.Gemini_Generated_Image_47v10s47v10s47v1;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(309, 221);
            Controls.Add(buttonCancel);
            Controls.Add(buttonChange);
            Controls.Add(textBoxNewPass);
            Controls.Add(labelNewPass);
            Controls.Add(labelTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePass";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Đổi mật khẩu";
            Load += ChangePass_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
