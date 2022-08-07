using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public User Following { get; set; }
        public int FollowingId { get; set; }

    }
}
