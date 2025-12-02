namespace plan_fighting_super_start
{
    public static class AccountData
    {
        public static string? Username = string.Empty;
        public static string? Password = string.Empty;
        public static string? Email = string.Empty;   // Email đã đăng ký

        public static int Gold;
        public static int UpgradeHP;
        public static int UpgradeDamage;
        public static int Level;

        public static bool RewardLv10Claimed;
        public static bool RewardLv50Claimed;
        public static bool RewardLv100Claimed;

        public static string? PlaneSkin = null;

        // 🔹 Trạng thái online hiện tại của account (cache ở client)
        public static bool IsOnline;
    }
}
