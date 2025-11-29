using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class ChatRieng
    {
        private System.ComponentModel.IContainer components = null;

        private RichTextBox rtbHopThoai;
        private TextBox txtNoiDung;
        private Button btnGui;
        private Label lblTieuDe;

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
            components = new System.ComponentModel.Container();
            rtbHopThoai = new RichTextBox();
            txtNoiDung = new TextBox();
            btnGui = new Button();
            lblTieuDe = new Label();
            SuspendLayout();
            // 
            // lblTieuDe
            // 
            lblTieuDe.Dock = DockStyle.Top;
            lblTieuDe.Height = 60;
            lblTieuDe.Text = "NHẮN VỚI ...";
            lblTieuDe.TextAlign = ContentAlignment.MiddleCenter;
            lblTieuDe.Font = new Font("Consolas", 20F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblTieuDe.ForeColor = Color.Cyan;
            lblTieuDe.BackColor = Color.FromArgb(5, 10, 25);
            // 
            // rtbHopThoai
            // 
            rtbHopThoai.BackColor = Color.FromArgb(10, 16, 32);
            rtbHopThoai.ForeColor = Color.White;
            rtbHopThoai.BorderStyle = BorderStyle.None;
            rtbHopThoai.ReadOnly = true;
            rtbHopThoai.Location = new Point(20, 70);
            rtbHopThoai.Name = "rtbHopThoai";
            rtbHopThoai.Size = new Size(560, 250);
            rtbHopThoai.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbHopThoai.Font = new Font("Consolas", 10.5F);
            // 
            // txtNoiDung
            // 
            txtNoiDung.BackColor = Color.FromArgb(10, 16, 32);
            txtNoiDung.ForeColor = Color.White;
            txtNoiDung.BorderStyle = BorderStyle.FixedSingle;
            txtNoiDung.Location = new Point(20, 340);
            txtNoiDung.Name = "txtNoiDung";
            txtNoiDung.Size = new Size(460, 27);
            txtNoiDung.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNoiDung.Font = new Font("Consolas", 10.5F);
            // 
            // btnGui
            // 
            btnGui.Text = "Gửi";
            btnGui.Name = "btnGui";
            btnGui.Font = new Font("Consolas", 10.5F, FontStyle.Bold);
            btnGui.ForeColor = Color.Cyan;
            btnGui.BackColor = Color.FromArgb(5, 10, 25);
            btnGui.FlatStyle = FlatStyle.Flat;
            btnGui.FlatAppearance.BorderColor = Color.Cyan;
            btnGui.Location = new Point(490, 338);
            btnGui.Size = new Size(90, 30);
            btnGui.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnGui.Click += btnGui_Click;
            // 
            // ChatRieng
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(5, 10, 25);
            ClientSize = new Size(600, 400);
            Controls.Add(btnGui);
            Controls.Add(txtNoiDung);
            Controls.Add(rtbHopThoai);
            Controls.Add(lblTieuDe);
            Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 163);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            Name = "ChatRieng";
            Text = "Nhắn tin riêng";
            Load += ChatRieng_Load;
            FormClosed += ChatRieng_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
