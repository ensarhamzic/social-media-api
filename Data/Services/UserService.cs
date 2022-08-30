using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
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
        private Account account;
        private Cloudinary cloudinary;

        public UserService(AppDbContext dbContext,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            account = new Account(
                configuration.GetSection("Cloudinary:Cloud").Value,
                configuration.GetSection("Cloudinary:ApiKey").Value,
                configuration.GetSection("Cloudinary:ApiSecret").Value);
            cloudinary = new Cloudinary(account);
        }

        public object Register(UserRegisterVM request)
        {
            bool userExists = dbContext.Users
                .Any(u => u.Email == request.Email || u.Username == request.Username);
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
            return new { user = newUser, token };
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

        public async Task<object> UpdateUserAsync(UserUpdateVM request)
        {
            var userId = GetAuthUserId();
            var foundUserByUsername = dbContext.Users
                .FirstOrDefault(u => u.Username == request.Username);

            var foundUserByEmail = dbContext.Users
                .FirstOrDefault(u => u.Email == request.Email);

            var authUser = GetAuthUserData();

            var canUpdateUsername = foundUserByUsername?.Id == null || (foundUserByUsername.Id == authUser.Id);
            var canUpdateEmail = foundUserByEmail?.Id == null || (foundUserByEmail.Id == authUser.Id);
            var canUpdate = canUpdateUsername && canUpdateEmail;

            if (canUpdate && authUser != null)
            {
                authUser.FirstName = request.FirstName;
                authUser.LastName = request.LastName;
                authUser.Username = request.Username;
                authUser.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Password))
                {
                    CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    authUser.PasswordHash = passwordHash;
                    authUser.PasswordSalt = passwordSalt;
                }

                var profilePicturePublicId = $"profile-pictures-test2/user{authUser.Id}_profile-picture";
                if (request.DeleteProfilePicture == true)
                {
                    var deletionParams = new DeletionParams(profilePicturePublicId)
                    {
                        ResourceType = ResourceType.Image
                    };
                    cloudinary.Destroy(deletionParams);
                    authUser.PictureURL = null;
                }
                else if (request.ProfilePicture != null)
                {
                    var filePath = Path.GetTempFileName();

                    using (var stream = File.Create(filePath))
                    {
                        await request.ProfilePicture.CopyToAsync(stream);
                    }
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(filePath),
                        PublicId = profilePicturePublicId,
                    };
                    var uploadResult = cloudinary.Upload(uploadParams);
                    authUser.PictureURL = uploadResult.Url.ToString();
                }

                dbContext.SaveChanges();
            }
            else
            {
                throw new Exception("User already exists");
            }

            string token = CreateToken(authUser);
            return new { user = authUser, token };
        }

        public object GetUserWithPosts(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var userWithPosts = dbContext.Users
                    .Include(u => u.Posts).ThenInclude(p => p.Comments).ThenInclude(c => c.User)
                    .Include(u => u.Posts).ThenInclude(p => p.Likes).ThenInclude(l => l.User)
                    .Include(u => u.Followers).ThenInclude(f => f.User)
                    .Include(u => u.Following).ThenInclude(f => f.Following)
                    .Where(u => u.Id == id)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.PictureURL,
                        following = u.Following.Select(f => FormatUserData(f.Following)),
                        followers = u.Followers.Select(f => FormatUserData(f.User)),
                        posts = u.Posts
                        .Select(p => new
                        {
                            p.Id,
                            user = FormatUserData(p.User),
                            p.Date,
                            p.Text,
                            likes = p.Likes
                                .Select(l => new
                                {
                                    l.PostId,
                                    user = FormatUserData(l.User),
                                }),
                            comments = p.Comments
                                .Select(c => new
                                {
                                    c.Id,
                                    c.UserId,
                                    c.PostId,
                                    c.Text,
                                    c.Date,
                                    user = FormatUserData(c.User),
                                })
                        })
                    }).FirstOrDefault();

                if (userWithPosts != null)
                {
                    return userWithPosts;
                }

            }
            throw new Exception("User not found");
        }

        public object FindUsers(string searchString)
        {
            var users = dbContext.Users
                .Where(u => u.FirstName.Contains(searchString)
                || u.LastName.Contains(searchString)
                || u.Username.Contains(searchString)
                ).Select(u => FormatUserData(u)).ToList();
            return users;
        }

       

        public object GetUserFeed()
        {
            var userId = GetAuthUserId();
            var userData = dbContext.Users
                .Include(u => u.Following).ThenInclude(f => f.Following).ThenInclude(f => f.Posts).ThenInclude(p => p.Comments)
                .Include(u => u.Following).ThenInclude(f => f.Following).ThenInclude(f => f.Posts).ThenInclude(p => p.Likes)
                .FirstOrDefault(u => u.Id == userId);
            var feed = new List<object>();
            foreach (var follow in userData.Following)
            {
                foreach (var post in follow.Following.Posts)
                {
                    feed.Add(new
                    {
                        user = new
                        {
                            id = follow.Following.Id,
                            username = follow.Following.Username,
                            pictureURL = follow.Following.PictureURL,
                        },

                        id = post.Id,
                        text = post.Text,
                        comments = post.Comments,
                        likes = post.Likes,
                        date = post.Date
                    });
                }
            }
            return feed;
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

        public string RemoveFollower(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var userId = GetAuthUserId();
                if (userId == id)
                    throw new Exception("Cannot unfollow yourself");
                var foundUser = dbContext.Users.FirstOrDefault(u => u.Id == id);
                if (foundUser == null)
                {
                    throw new Exception($"User with id of {stringId} is not found");
                }
                var foundFollow = dbContext.Follows
                    .FirstOrDefault(f => f.UserId == id && f.FollowingId == userId);
                if (foundFollow != null)
                {
                    dbContext.Follows.Remove(foundFollow);
                    dbContext.SaveChanges();
                    return "Removed follower";
                }
                else
                {
                    throw new Exception($"User with id of {id} does not follow you");
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

        public User GetAuthUserData()
        {
            var userId = GetAuthUserId();
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user;
        }


        private static object FormatUserData(User u)
        {
            return new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PictureURL
            };
        }

        
    }
}
