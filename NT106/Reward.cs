using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace plan_fighting_super_start
{
    public partial class Reward : Form
    {
        private class RewardInfo
        {
            public int Level { get; set; }
            public int DamageBonus { get; set; }
            public int GoldBonus { get; set; }
        }

        private readonly List<RewardInfo> _rewards = new List<RewardInfo>
        {
            new RewardInfo { Level = 10,  DamageBonus = 5,  GoldBonus = 100 },
            new RewardInfo { Level = 50, DamageBonus = 15, GoldBonus = 500 },
            new RewardInfo { Level = 100,DamageBonus = 30, GoldBonus = 2000 }
        };

        // ===== Màu UI giống Menu =====
        private readonly Color Teal = Color.FromArgb(0, 192, 192);
        private readonly Color BgDark = Color.FromArgb(10, 15, 30);
        private readonly Color BgPanel = Color.FromArgb(15, 22, 45);
        private readonly Color BgButton = Color.FromArgb(15, 25, 45);

        public Reward()
        {
            InitializeComponent();

            this.Load += Reward_Load;
            btnClaimReward.Click += BtnClaimReward_Click;
        }

        private void Reward_Load(object? sender, EventArgs e)
        {
            ApplyTheme();
            UpdateRewardUI();
        }

        // ===== Áp dụng theme cho form =====
        private void ApplyTheme()
        {
            // Nếu bạn vẫn muốn giữ BackgroundImage thì không cần set BackColor,
            // còn nếu muốn nền tối giống Menu thì uncomment dòng dưới:
            // this.BackgroundImage = null;
            this.BackColor = BgDark;

            // Label info
            labelInfo.BackColor = Color.Transparent;
            labelInfo.ForeColor = Teal;
            labelInfo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            // CheckedListBox
            checkedListRewards.BackColor = BgPanel;
            checkedListRewards.ForeColor = Teal;
            checkedListRewards.BorderStyle = BorderStyle.FixedSingle;

            // Button
            SetGameButton(btnClaimReward);
        }

        private void SetGameButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Teal;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 120, 140);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 100);
            button.BackColor = BgButton;
            button.ForeColor = Teal;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (_, __) =>
            {
                button.BackColor = Teal;
                button.ForeColor = Color.Black;
            };

            button.MouseLeave += (_, __) =>
            {
                button.BackColor = BgButton;
                button.ForeColor = Teal;
            };
        }

        // ===== Logic reward =====

        private bool IsLevelClaimed(int level)
        {
            switch (level)
            {
                case 10: return AccountData.RewardLv10Claimed;
                case 50: return AccountData.RewardLv50Claimed;
                case 100: return AccountData.RewardLv100Claimed;
                default: return true;
            }
        }

        private void SetLevelClaimed(int level)
        {
            switch (level)
            {
                case 10:
                    AccountData.RewardLv10Claimed = true;
                    break;
                case 50:
                    AccountData.RewardLv50Claimed = true;
                    break;
                case 100:
                    AccountData.RewardLv100Claimed = true;
                    break;
            }
        }

        private void UpdateRewardUI()
        {
            int level = AccountData.Level;

            // ----- Cập nhật danh sách checkbox -----
            checkedListRewards.Items.Clear();

            foreach (var r in _rewards)
            {
                bool claimed = IsLevelClaimed(r.Level);
                string text = $"Lv {r.Level}: +{r.DamageBonus} DMG, +{r.GoldBonus} Gold";

                int idx = checkedListRewards.Items.Add(text);
                checkedListRewards.SetItemChecked(idx, claimed); // đã nhận thì tick
            }

            checkedListRewards.Enabled = false; // chỉ hiển thị, không cho tự tick

            // ----- Label + nút -----
            bool anyAvailable = false;
            List<string> available = new List<string>();

            foreach (var r in _rewards)
            {
                if (level >= r.Level && !IsLevelClaimed(r.Level))
                {
                    anyAvailable = true;
                    available.Add(
                        $"Lv {r.Level}: +{r.DamageBonus} DMG, +{r.GoldBonus} Gold (chưa nhận)"
                    );
                }
            }

            if (!anyAvailable)
            {
                labelInfo.Text =
                    $"Level hiện tại: {level}\n" +
                    "Bạn không còn phần thưởng level nào chưa nhận.\n" +
                    "Mỗi mốc chỉ nhận được 1 lần.";
                btnClaimReward.Enabled = false;
            }
            else
            {
                labelInfo.Text =
                    $"Level hiện tại: {level}\n" +
                    "Các mốc có thể nhận thêm:\n" +
                    string.Join("\n", available) +
                    "\n\nNhấn nút để nhận tất cả phần thưởng chưa nhận.";
                btnClaimReward.Enabled = true;
            }
        }

        private void BtnClaimReward_Click(object? sender, EventArgs e)
        {
            int level = AccountData.Level;
            int totalDamage = 0;
            int totalGold = 0;
            List<string> detail = new List<string>();

            foreach (var r in _rewards)
            {
                if (level >= r.Level && !IsLevelClaimed(r.Level))
                {
                    totalDamage += r.DamageBonus;
                    totalGold += r.GoldBonus;
                    SetLevelClaimed(r.Level);

                    detail.Add($"Lv {r.Level}: +{r.DamageBonus} DMG, +{r.GoldBonus} Gold");
                }
            }

            if (totalDamage == 0 && totalGold == 0)
            {
                MessageBox.Show(
                    "Bạn không còn phần thưởng nào để nhận.",
                    "Reward",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                UpdateRewardUI();
                return;
            }

            AccountData.UpgradeDamage += totalDamage;
            AccountData.Gold += totalGold;

            Database.UpdateAccountData();

            MessageBox.Show(
                $"Nhận phần thưởng thành công!\n" +
                $"Tổng cộng: +{totalDamage} Damage, +{totalGold} Gold\n\n" +
                string.Join("\n", detail),
                "Reward",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            UpdateRewardUI();
        }
    }
}
