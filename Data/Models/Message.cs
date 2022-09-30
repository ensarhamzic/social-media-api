namespace SocialMediaAPI.Data.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int FromUserId { get; set; }
        public User FromUser { get; set; }

        public int ToUserId { get; set; }
        public User ToUser { get; set; }

        public string Text { get; set; } = string.Empty;
        public DateTime TimeSent { get; set; }
    }
}
