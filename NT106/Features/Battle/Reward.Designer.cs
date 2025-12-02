using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class Reward
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Panel panelMain;
        private Panel panelCard;
        private Label labelTitle;
        private Label labelInfo;
        private CheckedListBox checkedListRewards;
        private Button btnClaimReward;

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
            components = new System.ComponentModel.Container();
            panelMain = new Panel();
            panelCard = new Panel();
            labelTitle = new Label();
            labelInfo = new Label();
            checkedListRewards = new CheckedListBox();
            btnClaimReward = new Button();

            panelMain.SuspendLayout();
            panelCard.SuspendLayout();
            SuspendLayout();

            // 
            // Reward (Form)
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Reward";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Phần thưởng Level";

            // Nếu resource này có thật thì để, không thì comment dòng dưới lại:
            // BackgroundImage = Properties.Resource.Gemini_Generated_Image_5ka7of5ka7of5ka7;
            BackgroundImageLayout = ImageLayout.Stretch;

            // 
            // panelMain
            // 
            panelMain.Dock = DockStyle.Fill;
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(16);

            // 
            // panelCard
            // 
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(440, 280);
            panelCard.Location = new Point(20, 20);
            panelCard.BackColor = Color.FromArgb(15, 22, 45);

            // 
            // labelTitle
            // 
            labelTitle.Name = "labelTitle";
            labelTitle.Text = "🎁 PHẦN THƯỞNG LEVEL";
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            labelTitle.Location = new Point(12, 10);
            labelTitle.Size = new Size(416, 32);

            // 
            // labelInfo
            // 
            labelInfo.Name = "labelInfo";
            labelInfo.Text = "Thông tin phần thưởng sẽ hiển thị ở đây.";
            labelInfo.Location = new Point(12, 50);
            labelInfo.Size = new Size(416, 80);

            // 
            // checkedListRewards
            // 
            checkedListRewards.Name = "checkedListRewards";
            checkedListRewards.Location = new Point(12, 135);
            checkedListRewards.Size = new Size(416, 88);
            checkedListRewards.BorderStyle = BorderStyle.None;
            checkedListRewards.CheckOnClick = false;

            // 
            // btnClaimReward
            // 
            btnClaimReward.Name = "btnClaimReward";
            btnClaimReward.Text = "Nhận tất cả phần thưởng";
            btnClaimReward.Size = new Size(220, 42);
            btnClaimReward.Location = new Point(
                (440 - 220) / 2,     // center theo chiều ngang card
                440 > 0 ? 235 : 235  // fix số, không phụ thuộc Layout event
            );
            btnClaimReward.TabIndex = 1;
            btnClaimReward.UseVisualStyleBackColor = true;

            // 
            // Add controls
            // 
            panelCard.Controls.Add(btnClaimReward);
            panelCard.Controls.Add(checkedListRewards);
            panelCard.Controls.Add(labelInfo);
            panelCard.Controls.Add(labelTitle);

            panelMain.Controls.Add(panelCard);
            Controls.Add(panelMain);

            panelMain.ResumeLayout(false);
            panelCard.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
