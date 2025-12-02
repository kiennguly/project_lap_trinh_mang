namespace plan_fighting_super_start
{
    // Model 1 bản ghi bạn bè trả về từ API Friend
    public class FriendEntry
    {
        public string Username { get; set; } = "";
        public string Status { get; set; } = "";       // pending / sent / accepted
        public string AvatarKey { get; set; } = "";    // avatars/{username}.png (nếu cần)
    }
}
