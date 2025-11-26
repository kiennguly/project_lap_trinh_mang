using System.Drawing;
using System.Windows.Forms;
using Font = System.Drawing.Font;

namespace plan_fighting_super_start
{
    partial class GAMEBOSS : Form
    {
        private System.ComponentModel.IContainer components = null;

        private PictureBox player;
        private PictureBox boss;
        private PictureBox playerBullet;
        private Label txtScore;
        private ProgressBar playerHealthBar;
        private ProgressBar bossHealthBar;
        private Button buttonExit;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer survivalTimer;

        private Panel pausePanel;
        private Label pauseTextLabel;
        private Button btnResume;
        private Button btnPauseExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GAMEBOSS));
            player = new PictureBox();
            boss = new PictureBox();
            playerBullet = new PictureBox();
            txtScore = new Label();
            playerHealthBar = new ProgressBar();
            bossHealthBar = new ProgressBar();
            buttonExit = new Button();
            gameTimer = new System.Windows.Forms.Timer(components);
            survivalTimer = new System.Windows.Forms.Timer(components);
            pausePanel = new Panel();
            btnPauseExit = new Button();
            btnResume = new Button();
            pauseTextLabel = new Label();
            lblHint = new Label();
            ((System.ComponentModel.ISupportInitialize)player).BeginInit();
            ((System.ComponentModel.ISupportInitialize)boss).BeginInit();
            ((System.ComponentModel.ISupportInitialize)playerBullet).BeginInit();
            pausePanel.SuspendLayout();
            SuspendLayout();
            // 
            // player
            // 
            player.BackColor = Color.Transparent;
            player.BackgroundImageLayout = ImageLayout.Zoom;
            player.Image = (Image)resources.GetObject("player.Image");
            player.Location = new Point(350, 520);
            player.Name = "player";
            player.Size = new Size(62, 80);
            player.SizeMode = PictureBoxSizeMode.StretchImage;
            player.TabIndex = 0;
            player.TabStop = false;
            // 
            // boss
            // 
            boss.BackColor = Color.Transparent;
            boss.BackgroundImageLayout = ImageLayout.Stretch;
            boss.Image = (Image)resources.GetObject("boss.Image");
            boss.Location = new Point(340, 50);
            boss.Name = "boss";
            boss.Size = new Size(120, 100);
            boss.SizeMode = PictureBoxSizeMode.StretchImage;
            boss.TabIndex = 1;
            boss.TabStop = false;
            boss.Click += boss_Click;
            // 
            // playerBullet
            // 
            playerBullet.BackColor = Color.Transparent;
            playerBullet.Location = new Point(-50, -50);
            playerBullet.Name = "playerBullet";
            playerBullet.Size = new Size(12, 30);
            playerBullet.TabIndex = 2;
            playerBullet.TabStop = false;
            playerBullet.Visible = false;
            // 
            // txtScore
            // 
            txtScore.BackColor = Color.Transparent;
            txtScore.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            txtScore.ForeColor = Color.FromArgb(0, 192, 192);
            txtScore.Location = new Point(0, 36);
            txtScore.Name = "txtScore";
            txtScore.Size = new Size(800, 30);
            txtScore.TabIndex = 3;
            txtScore.Text = "Gold: 0  Time: 90  Level: 1";
            txtScore.TextAlign = ContentAlignment.MiddleCenter;
            txtScore.Click += txtScore_Click;
            // 
            // playerHealthBar
            // 
            playerHealthBar.ForeColor = Color.Lime;
            playerHealthBar.Location = new Point(10, 590);
            playerHealthBar.Name = "playerHealthBar";
            playerHealthBar.Size = new Size(250, 20);
            playerHealthBar.TabIndex = 4;
            // 
            // bossHealthBar
            // 
            bossHealthBar.ForeColor = Color.Red;
            bossHealthBar.Location = new Point(540, 12);
            bossHealthBar.Name = "bossHealthBar";
            bossHealthBar.Size = new Size(250, 20);
            bossHealthBar.TabIndex = 5;
            // 
            // buttonExit
            // 
            buttonExit.FlatStyle = FlatStyle.Flat;
            buttonExit.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            buttonExit.ForeColor = Color.FromArgb(0, 192, 192);
            buttonExit.Location = new Point(340, 310);
            buttonExit.Name = "buttonExit";
            buttonExit.Size = new Size(140, 40);
            buttonExit.TabIndex = 6;
            buttonExit.TabStop = false;
            buttonExit.Text = "Thoát";
            buttonExit.UseVisualStyleBackColor = true;
            buttonExit.Visible = false;
            buttonExit.Click += buttonExit_Click;
            // 
            // gameTimer
            // 
            gameTimer.Interval = 20;
            gameTimer.Tick += mainGameTimerEvent;
            // 
            // survivalTimer
            // 
            survivalTimer.Interval = 1000;
            survivalTimer.Tick += survivalTimer_Tick;
            // 
            // pausePanel
            // 
            pausePanel.BackColor = Color.FromArgb(180, 0, 0, 0);
            pausePanel.Controls.Add(btnPauseExit);
            pausePanel.Controls.Add(btnResume);
            pausePanel.Controls.Add(pauseTextLabel);
            pausePanel.Location = new Point(200, 220);
            pausePanel.Name = "pausePanel";
            pausePanel.Size = new Size(400, 150);
            pausePanel.TabIndex = 8;
            pausePanel.Visible = false;
            // 
            // btnPauseExit
            // 
            btnPauseExit.BackColor = Color.FromArgb(20, 30, 50);
            btnPauseExit.FlatStyle = FlatStyle.Flat;
            btnPauseExit.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnPauseExit.ForeColor = Color.FromArgb(0, 192, 192);
            btnPauseExit.Location = new Point(220, 80);
            btnPauseExit.Name = "btnPauseExit";
            btnPauseExit.Size = new Size(120, 40);
            btnPauseExit.TabIndex = 2;
            btnPauseExit.TabStop = false;
            btnPauseExit.Text = "Thoát";
            btnPauseExit.UseVisualStyleBackColor = false;
            btnPauseExit.Click += btnPauseExit_Click;
            // 
            // btnResume
            // 
            btnResume.BackColor = Color.FromArgb(20, 30, 50);
            btnResume.FlatStyle = FlatStyle.Flat;
            btnResume.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnResume.ForeColor = Color.FromArgb(0, 192, 192);
            btnResume.Location = new Point(60, 80);
            btnResume.Name = "btnResume";
            btnResume.Size = new Size(120, 40);
            btnResume.TabIndex = 1;
            btnResume.TabStop = false;
            btnResume.Text = "Tiếp tục";
            btnResume.UseVisualStyleBackColor = false;
            btnResume.Click += btnResume_Click;
            // 
            // pauseTextLabel
            // 
            pauseTextLabel.BackColor = Color.Transparent;
            pauseTextLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 163);
            pauseTextLabel.ForeColor = Color.FromArgb(0, 192, 192);
            pauseTextLabel.Location = new Point(10, 10);
            pauseTextLabel.Name = "pauseTextLabel";
            pauseTextLabel.Size = new Size(380, 40);
            pauseTextLabel.TabIndex = 0;
            pauseTextLabel.Text = "⏸ TẠM DỪNG";
            pauseTextLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.BackColor = Color.Transparent;
            lblHint.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHint.ForeColor = Color.FromArgb(0, 192, 192);
            lblHint.Location = new Point(10, 10);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(257, 23);
            lblHint.TabIndex = 9;
            lblHint.Text = "Nhấn P để tạm dừng / tiếp tục";
            // 
            // GAMEBOSS
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImage = Properties.Resource.NenBOSS;
            ClientSize = new Size(800, 620);
            Controls.Add(pausePanel);
            Controls.Add(buttonExit);
            Controls.Add(bossHealthBar);
            Controls.Add(playerHealthBar);
            Controls.Add(txtScore);
            Controls.Add(playerBullet);
            Controls.Add(boss);
            Controls.Add(player);
            Controls.Add(lblHint);
            DoubleBuffered = true;
            KeyPreview = true;
            Name = "GAMEBOSS";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Boss Shooter Game";
            FormClosed += GAMEBOSS_FormClosed;
            Load += Form4_Load;
            Shown += GAMEBOSS_Shown;
            KeyDown += keyisdown;
            KeyUp += keyisup;
            ((System.ComponentModel.ISupportInitialize)player).EndInit();
            ((System.ComponentModel.ISupportInitialize)boss).EndInit();
            ((System.ComponentModel.ISupportInitialize)playerBullet).EndInit();
            pausePanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
        private Label lblHint;
    }
}
