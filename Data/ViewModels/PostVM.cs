using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class PostVM
    {
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}
