using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class CommentVM
    {
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}
