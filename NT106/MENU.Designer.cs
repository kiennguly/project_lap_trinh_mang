using System.Windows.Forms;
using Font = System.Drawing.Font;
using System.Drawing;

namespace plan_fighting_super_start
{
    partial class Menu
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox textBoxGold;
        private Button buttonPlay;
        private Button buttonUpgradeHP;
        private Button buttonUpgradeDamage;
        private Button buttonExit;
        private Label label1;
        private Label label2;
        private TextBox textBox1;  // HP
        private Label label3;
        private TextBox textBox2;  // Damage
        private Button button1;    // Chơi với người
        private TextBox textBox3;  // Level
        private Label label4;
        private Label labelWelcome;
        private SaveFileDialog saveFileDialog1;
        private Button button2;    // Rank
        private Button button4;    // 🎁
        private Button button5;    // code

        private PictureBox pictureBoxAvatar;
        private PictureBox pictureBoxPlane;
        private Button buttonDoiMayBay;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            textBoxGold = new TextBox();
            buttonPlay = new Button();
            buttonUpgradeHP = new Button();
            buttonUpgradeDamage = new Button();
            buttonExit = new Button();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            textBox2 = new TextBox();
            button1 = new Button();
            textBox3 = new TextBox();
            label4 = new Label();
            labelWelcome = new Label();
            saveFileDialog1 = new SaveFileDialog();
            button2 = new Button();
            button4 = new Button();
            button5 = new Button();
            pictureBoxAvatar = new PictureBox();
            pictureBoxPlane = new PictureBox();
            buttonDoiMayBay = new Button();
            label5 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAvatar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPlane).BeginInit();
            SuspendLayout();
            // 
            // textBoxGold
            // 
            textBoxGold.Location = new Point(324, 283);
            textBoxGold.Name = "textBoxGold";
            textBoxGold.Size = new Size(160, 27);
            textBoxGold.TabIndex = 16;
            // 
            // buttonPlay
            // 
            buttonPlay.FlatStyle = FlatStyle.Flat;
            buttonPlay.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonPlay.ForeColor = Color.FromArgb(0, 192, 192);
            buttonPlay.Location = new Point(179, 423);
            buttonPlay.Name = "buttonPlay";
            buttonPlay.Size = new Size(160, 46);
            buttonPlay.TabIndex = 11;
            buttonPlay.Text = "Chơi BOSS";
            buttonPlay.UseVisualStyleBackColor = true;
            buttonPlay.Click += buttonPlay_Click;
            // 
            // buttonUpgradeHP
            // 
            buttonUpgradeHP.FlatStyle = FlatStyle.Flat;
            buttonUpgradeHP.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonUpgradeHP.ForeColor = Color.FromArgb(0, 192, 192);
            buttonUpgradeHP.Location = new Point(389, 483);
            buttonUpgradeHP.Name = "buttonUpgradeHP";
            buttonUpgradeHP.Size = new Size(180, 46);
            buttonUpgradeHP.TabIndex = 8;
            buttonUpgradeHP.Text = "Nâng HP";
            buttonUpgradeHP.UseVisualStyleBackColor = true;
            buttonUpgradeHP.Click += buttonUpgradeHP_Click;
            // 
            // buttonUpgradeDamage
            // 
            buttonUpgradeDamage.FlatStyle = FlatStyle.Flat;
            buttonUpgradeDamage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonUpgradeDamage.ForeColor = Color.FromArgb(0, 192, 192);
            buttonUpgradeDamage.Location = new Point(179, 483);
            buttonUpgradeDamage.Name = "buttonUpgradeDamage";
            buttonUpgradeDamage.Size = new Size(160, 46);
            buttonUpgradeDamage.TabIndex = 9;
            buttonUpgradeDamage.Text = "Nâng Damage";
            buttonUpgradeDamage.UseVisualStyleBackColor = true;
            buttonUpgradeDamage.Click += buttonUpgradeDamage_Click;
            // 
            // buttonExit
            // 
            buttonExit.FlatStyle = FlatStyle.Flat;
            buttonExit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonExit.ForeColor = Color.FromArgb(0, 192, 192);
            buttonExit.Location = new Point(389, 543);
            buttonExit.Name = "buttonExit";
            buttonExit.Size = new Size(180, 46);
            buttonExit.TabIndex = 7;
            buttonExit.Text = "Thoát";
            buttonExit.UseVisualStyleBackColor = true;
            buttonExit.Click += buttonExit_Click;
            // 
            // label1
            // 
            label1.Location = new Point(189, 283);
            label1.Name = "label1";
            label1.Size = new Size(100, 28);
            label1.TabIndex = 17;
            label1.Text = "Vàng";
            // 
            // label2
            // 
            label2.Location = new Point(189, 328);
            label2.Name = "label2";
            label2.Size = new Size(100, 28);
            label2.TabIndex = 15;
            label2.Text = "HP";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(324, 328);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(160, 27);
            textBox1.TabIndex = 14;
            // 
            // label3
            // 
            label3.Location = new Point(189, 373);
            label3.Name = "label3";
            label3.Size = new Size(100, 28);
            label3.TabIndex = 13;
            label3.Text = "DAMAGE Plus";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(324, 373);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(160, 27);
            textBox2.TabIndex = 12;
            // 
            // button1
            // 
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button1.ForeColor = Color.FromArgb(0, 192, 192);
            button1.Location = new Point(389, 423);
            button1.Name = "button1";
            button1.Size = new Size(180, 46);
            button1.TabIndex = 10;
            button1.Text = "Chơi với người";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(324, 238);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(160, 27);
            textBox3.TabIndex = 18;
            // 
            // label4
            // 
            label4.Location = new Point(189, 238);
            label4.Name = "label4";
            label4.Size = new Size(100, 28);
            label4.TabIndex = 19;
            label4.Text = "Level";
            // 
            // labelWelcome
            // 
            labelWelcome.Location = new Point(93, 9);
            labelWelcome.Name = "labelWelcome";
            labelWelcome.Size = new Size(483, 46);
            labelWelcome.TabIndex = 20;
            labelWelcome.Text = "Xin chào";
            labelWelcome.Click += labelWelcome_Click;
            // 
            // button2
            // 
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button2.ForeColor = Color.FromArgb(0, 192, 192);
            button2.Location = new Point(179, 543);
            button2.Name = "button2";
            button2.Size = new Size(160, 46);
            button2.TabIndex = 6;
            button2.Text = "Rank";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.Transparent;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            button4.ForeColor = Color.FromArgb(0, 192, 192);
            button4.Location = new Point(610, 10);
            button4.Name = "button4";
            button4.Size = new Size(60, 28);
            button4.TabIndex = 4;
            button4.Text = "🎁";
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.BackColor = Color.Transparent;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            button5.ForeColor = Color.FromArgb(0, 192, 192);
            button5.Location = new Point(12, 10);
            button5.Name = "button5";
            button5.Size = new Size(75, 28);
            button5.TabIndex = 3;
            button5.Text = "Account";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // pictureBoxAvatar
            // 
            pictureBoxAvatar.BackColor = Color.FromArgb(15, 22, 45);
            pictureBoxAvatar.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxAvatar.Location = new Point(160, 82);
            pictureBoxAvatar.Name = "pictureBoxAvatar";
            pictureBoxAvatar.Size = new Size(110, 100);
            pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAvatar.TabIndex = 2;
            pictureBoxAvatar.TabStop = false;
            // 
            // pictureBoxPlane
            // 
            pictureBoxPlane.BackColor = Color.FromArgb(15, 22, 45);
            pictureBoxPlane.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxPlane.Location = new Point(360, 82);
            pictureBoxPlane.Name = "pictureBoxPlane";
            pictureBoxPlane.Size = new Size(170, 100);
            pictureBoxPlane.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPlane.TabIndex = 1;
            pictureBoxPlane.TabStop = false;
            // 
            // buttonDoiMayBay
            // 
            buttonDoiMayBay.FlatStyle = FlatStyle.Flat;
            buttonDoiMayBay.Location = new Point(274, 614);
            buttonDoiMayBay.Name = "buttonDoiMayBay";
            buttonDoiMayBay.Size = new Size(180, 40);
            buttonDoiMayBay.TabIndex = 0;
            buttonDoiMayBay.Text = "ĐỔI MÁY BAY";
            buttonDoiMayBay.UseVisualStyleBackColor = true;
            buttonDoiMayBay.Click += buttonDoiMayBay_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label5.ForeColor = Color.FromArgb(0, 192, 192);
            label5.Location = new Point(415, 59);
            label5.Name = "label5";
            label5.Size = new Size(68, 20);
            label5.TabIndex = 21;
            label5.Text = "Máy bay";
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 30);
            BackgroundImage = Properties.Resource.Gemini_Generated_Image_dy9x6hdy9x6hdy9x;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(684, 680);
            Controls.Add(label5);
            Controls.Add(buttonDoiMayBay);
            Controls.Add(pictureBoxPlane);
            Controls.Add(pictureBoxAvatar);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(buttonExit);
            Controls.Add(buttonUpgradeHP);
            Controls.Add(buttonUpgradeDamage);
            Controls.Add(button1);
            Controls.Add(buttonPlay);
            Controls.Add(textBox2);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(textBoxGold);
            Controls.Add(label1);
            Controls.Add(textBox3);
            Controls.Add(label4);
            Controls.Add(labelWelcome);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Menu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menu Game";
            Load += Form3_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxAvatar).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPlane).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label5;
    }
}
