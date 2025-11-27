using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class GAMESOLO
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnExit;
        private Label lblStatusGame;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            btnExit = new Button();
            lblStatusGame = new Label();
            SuspendLayout();
            // 
            // btnExit
            // 
            btnExit.Location = new Point(11, 13);
            btnExit.Margin = new Padding(3, 4, 3, 4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(91, 40);
            btnExit.TabIndex = 0;
            btnExit.TabStop = false;
            btnExit.Text = "Thoát trận";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Visible = false;
            btnExit.Click += btnExit_Click;
            btnExit.PreviewKeyDown += AnyControl_PreviewKeyDown;
            // 
            // lblStatusGame
            // 
            lblStatusGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblStatusGame.AutoSize = true;
            lblStatusGame.BackColor = Color.Transparent;
            lblStatusGame.ForeColor = Color.White;
            lblStatusGame.Location = new Point(901, 771);
            lblStatusGame.Name = "lblStatusGame";
            lblStatusGame.Size = new Size(116, 20);
            lblStatusGame.TabIndex = 1;
            lblStatusGame.Text = "Đang chuẩn bị…";
            lblStatusGame.PreviewKeyDown += AnyControl_PreviewKeyDown;
            // 
            // GAMESOLO
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImage = Properties.Resource.nensolo;

            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1029, 800);
            Controls.Add(lblStatusGame);
            Controls.Add(btnExit);
            DoubleBuffered = true;
            KeyPreview = true;
            Margin = new Padding(3, 4, 3, 4);
            Name = "GAMESOLO";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LAN Shooting Game";
            FormClosing += Form6_FormClosing;
            Load += GAMESOLO_Load;
            KeyDown += GAMESOLO_KeyDown;
            KeyUp += GAMESOLO_KeyUp;
            PreviewKeyDown += AnyControl_PreviewKeyDown;
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
    }
}
