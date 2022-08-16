using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public Post Post { get; set; }
        public int PostId { get; set; }
    }
}
