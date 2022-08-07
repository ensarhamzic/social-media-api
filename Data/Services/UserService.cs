using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;
using System.Security.Cryptography;

namespace SocialMediaAPI.Data.Services
{
    public class UserService
    {
        private AppDbContext dbContext;
        public UserService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Register(UserRegisterVM request)
        {
            bool userExists = dbContext.Users.Any(u => u.Email == request.Email || u.Username == request.Username );
            if(userExists)
            {
                throw new Exception("User already exists");
            }
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var newUser = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };
            dbContext.Users.Add(newUser);
            dbContext.SaveChanges();


            return "token";
        }

        public string Login(UserLoginVM request)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Username == request.Username);
            var failedResponse = "Check your credentials and try again!";
            if(user == null)
                throw new Exception(failedResponse);
            var isPasswordCorrect = VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!isPasswordCorrect)
                throw new Exception(failedResponse);
            return "token";
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
