using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    partial class Reward
    {
        private System.ComponentModel.IContainer components = null;

        private Label labelInfo;
        private Button btnClaimReward;
        private CheckedListBox checkedListRewards;

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
            labelInfo = new Label();
            btnClaimReward = new Button();
            checkedListRewards = new CheckedListBox();
            SuspendLayout();
            // 
            // labelInfo
            // 
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelInfo.Location = new Point(14, 12);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(411, 70);
            labelInfo.TabIndex = 0;
            labelInfo.Text = "Thông tin phần thưởng sẽ hiển thị ở đây.";
            // 
            // btnClaimReward
            // 
            btnClaimReward.Anchor = AnchorStyles.Bottom;
            btnClaimReward.Location = new Point(137, 200);
            btnClaimReward.Name = "btnClaimReward";
            btnClaimReward.Size = new Size(171, 45);
            btnClaimReward.TabIndex = 1;
            btnClaimReward.Text = "Nhận phần thưởng";
            btnClaimReward.UseVisualStyleBackColor = true;
            // 
            // checkedListRewards
            // 
            checkedListRewards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            checkedListRewards.FormattingEnabled = true;
            checkedListRewards.Location = new Point(14, 90);
            checkedListRewards.Name = "checkedListRewards";
            checkedListRewards.Size = new Size(411, 92);
            checkedListRewards.TabIndex = 2;
            // 
            // Reward
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resource.Gemini_Generated_Image_5ka7of5ka7of5ka7;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(439, 261);
            Controls.Add(checkedListRewards);
            Controls.Add(btnClaimReward);
            Controls.Add(labelInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Reward";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Phần thưởng Level";
            ResumeLayout(false);
        }

        #endregion
    }
}
