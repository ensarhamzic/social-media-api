using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SocialMediaAPI.Data.Services
{
    public class UserService
    {
        private AppDbContext dbContext;
        private IConfiguration configuration;
        private IHttpContextAccessor httpContextAccessor;
        public UserService(AppDbContext dbContext,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public object Register(UserRegisterVM request)
        {
            bool userExists = dbContext.Users.Any(u => u.Email == request.Email || u.Username == request.Username);
            if (userExists)
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

            string token = CreateToken(newUser);
            return new { user = newUser, token};
        }

        public User GetAuthUserData()
        {
            var userId = GetAuthUserId();
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user;
        }

        public object Login(UserLoginVM request)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Username == request.Username);
            var failedResponse = "Check your credentials and try again!";
            if (user == null)
                throw new Exception(failedResponse);
            var isPasswordCorrect = VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!isPasswordCorrect)
                throw new Exception(failedResponse);
            string token = CreateToken(user);
            return new { user, token };
        }

        public User GetUserWithPosts(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var userWithPosts = dbContext.Users
                    .Include(u => u.Posts).FirstOrDefault(u => u.Id == id);

                if (userWithPosts != null)
                {
                    var userId = GetAuthUserId();
                    var follow = dbContext.Follows
                    .FirstOrDefault(f => f.UserId == userId
                    && f.FollowingId == userWithPosts.Id);
                    if (follow != null || userWithPosts.Id == userId)
                        return userWithPosts;
                    else
                        throw new Exception("You don't follow that user");
                }

            }
            throw new Exception("User not found");
        }

        public string FollowOrUnfollowUser(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var userId = GetAuthUserId();
                if (userId == id)
                    throw new Exception("Cannot follow/unfollow yourself");
                var foundUser = dbContext.Users.FirstOrDefault(u => u.Id == id);
                if (foundUser == null)
                {
                    throw new Exception($"User with id of {stringId} is not found");
                }
                var foundFollow = dbContext.Follows
                    .FirstOrDefault(f => f.UserId == userId && f.FollowingId == id);
                if (foundFollow != null)
                {
                    dbContext.Follows.Remove(foundFollow);
                    dbContext.SaveChanges();
                    return "User unfollowed";
                }
                else
                {
                    var newFollow = new Follow()
                    {
                        UserId = userId,
                        FollowingId = id
                    };
                    dbContext.Follows.Add(newFollow);
                    dbContext.SaveChanges();
                    return "User followed";
                }
            }
            throw new Exception($"User with id of {stringId} is not found");
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

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.
                GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CheckId(string stringId, out int id, out bool isValid)
        {
            bool valid = int.TryParse(stringId, out int convertedId);
            isValid = valid;
            id = convertedId;
        }

        private int GetAuthUserId()
        {
            return int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
