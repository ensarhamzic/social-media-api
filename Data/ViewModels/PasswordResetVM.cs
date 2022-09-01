using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class PasswordResetVM
    {
        public string ResetToken { get; set; }
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}
