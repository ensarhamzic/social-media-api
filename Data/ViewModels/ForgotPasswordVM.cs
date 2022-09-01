using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class ForgotPasswordVM
    {
        [EmailAddress]
        public string Email { get; set; } 
    }
}
