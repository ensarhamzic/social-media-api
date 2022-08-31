using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class UserRegisterVM : IValidatableObject
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords don't match");
            }
        }
    }
}
