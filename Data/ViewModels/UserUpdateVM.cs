using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Data.ViewModels
{
    public class UserUpdateVM : IValidatableObject
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
        [MinLength(8)]
        public string? Password { get; set; }
        public bool? DeleteProfilePicture { get; set; }
        public IFormFile? ProfilePicture { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProfilePicture != null)
            {
                var validExtensions = new List<string>()
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };
                var extension = Path.GetExtension(ProfilePicture.FileName);
                if(!validExtensions.Contains(extension.ToLower()))
                {
                    yield return new ValidationResult($"File format must be either .jpg, .jpeg or .png");
                }
            }
        }
    }
}
