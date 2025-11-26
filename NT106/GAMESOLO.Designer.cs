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
            this.btnExit = new System.Windows.Forms.Button();
            this.lblStatusGame = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(11, 13);
            this.btnExit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(91, 40);
            this.btnExit.TabIndex = 0;
            this.btnExit.TabStop = false;
            this.btnExit.Text = "Thoát";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Visible = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.btnExit.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.AnyControl_PreviewKeyDown);
            // 
            // lblStatusGame
            // 
            this.lblStatusGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusGame.AutoSize = true;
            this.lblStatusGame.BackColor = System.Drawing.Color.Transparent;
            this.lblStatusGame.ForeColor = System.Drawing.Color.White;
            this.lblStatusGame.Location = new System.Drawing.Point(901, 771);
            this.lblStatusGame.Name = "lblStatusGame";
            this.lblStatusGame.Size = new System.Drawing.Size(116, 20);
            this.lblStatusGame.TabIndex = 1;
            this.lblStatusGame.Text = "Đang chuẩn bị…";
            this.lblStatusGame.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.AnyControl_PreviewKeyDown);
            // 
            // GAMESOLO
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1029, 800);
            this.Controls.Add(this.lblStatusGame);
            this.Controls.Add(this.btnExit);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "GAMESOLO";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LAN Shooting Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form6_FormClosing);
            this.Load += new System.EventHandler(this.GAMESOLO_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GAMESOLO_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GAMESOLO_KeyUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.AnyControl_PreviewKeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}
