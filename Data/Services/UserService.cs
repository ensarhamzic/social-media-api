using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Text;
using MimeKit;
using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using MailKit.Net.Smtp;

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

        public string Register(UserRegisterVM request)
        {
            bool userExists = dbContext.Users
                .Any(u => u.Email == request.Email || u.Username == request.Username);
            if (userExists)
            {
                throw new Exception("User already exists");
            }
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            // add new user
            var newUser = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Verified = false,
            };
            dbContext.Users.Add(newUser);
            dbContext.SaveChanges();
            // create verification token
            var newEmailVerification = new Verification()
            {
                UserId = newUser.Id,
                Token = Guid.NewGuid().ToString(),
            };
            dbContext.Verifications.Add(newEmailVerification);
            dbContext.SaveChanges();

            // send verification email
            var emailText = $"<h1>Welcome to Social Media App</h1>" +
                $"<h3>Please click " +
                $"<a href=\"{configuration.GetSection("ClientAppUrl").Value}/verify/{newEmailVerification.Token}\">here</a>" +
                $" to confirm your account</h3>";
            SendEmail(newUser.Email, "Confirm your account", emailText);

            return "User successfully created";
        }



        public string ConfirmAccount(string confirmToken)
        {
            // check verification token and verify user
            var foundVerification = dbContext.Verifications.FirstOrDefault(v => v.Token == confirmToken);
            if (foundVerification == null) throw new Exception("Invalid token");
            var foundUser = dbContext.Users.FirstOrDefault(u => u.Id == foundVerification.UserId);
            foundUser.Verified = true;
            dbContext.Verifications.Remove(foundVerification);
            dbContext.SaveChanges();
            return "User verified";
        }

        public string ForgotPassword(string email)
        {
            var foundUser = dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (foundUser == null) throw new Exception("User with that email is not found");
            // generates forgot password token
            var newPasswordReset = new PasswordReset()
            {
                UserId = foundUser.Id,
                Token = Guid.NewGuid().ToString()
            };
            dbContext.PasswordResets.Add(newPasswordReset);
            dbContext.SaveChanges();
            // sends email to user
            var emailText = $"<h1>Reset your password</h1>" +
                        $"<h3>Please click " +
                        $"<a href=\"{configuration.GetSection("ClientAppUrl").Value}/reset-password/{newPasswordReset.Token}\">here</a>" +
                        $" to reset your password</h3>";
            SendEmail(foundUser.Email, "Reset password", emailText);
            return "Token created";
        }

        public string ResetPassword(string resetToken, string newPassword)
        {
            // verifies reset token and updates user password
            var foundPasswordReset = dbContext.PasswordResets.FirstOrDefault(p => p.Token == resetToken);
            if (foundPasswordReset == null) throw new Exception("Invalid token");
            var foundUser = dbContext.Users.FirstOrDefault(u => u.Id == foundPasswordReset.UserId);
            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            foundUser.PasswordHash = passwordHash;
            foundUser.PasswordSalt = passwordSalt;
            dbContext.PasswordResets.Remove(foundPasswordReset);
            dbContext.SaveChanges();
            return "Password reset completed";
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
            if (!user.Verified)
                throw new Exception("Verify your account first!");
            string token = CreateToken(user);
            return new { user, token };
        }

        public async Task<object> UpdateUser(UserUpdateVM request)
        {
            var userId = GetAuthUserId();
            var foundUserByUsername = dbContext.Users
                .FirstOrDefault(u => u.Username == request.Username);

            var foundUserByEmail = dbContext.Users
                .FirstOrDefault(u => u.Email == request.Email);

            var authUser = GetAuthUserData();

            // can user update username or email (does someone else have same email or username)
            var canUpdateUsername = foundUserByUsername?.Id == null || (foundUserByUsername.Id == authUser.Id);
            var canUpdateEmail = foundUserByEmail?.Id == null || (foundUserByEmail.Id == authUser.Id);
            var canUpdate = canUpdateUsername && canUpdateEmail;

            if (canUpdate && authUser != null)
            {
                authUser.FirstName = request.FirstName;
                authUser.LastName = request.LastName;
                authUser.Username = request.Username;
                // if user enters new email, send verificaion link to that new email
                if (authUser.Email != request.Email)
                {
                    authUser.Email = request.Email;
                    authUser.Verified = false;
                    var newEmailVerification = new Verification()
                    {
                        UserId = authUser.Id,
                        Token = Guid.NewGuid().ToString(),
                    };
                    dbContext.Verifications.Add(newEmailVerification);
                    var emailText = $"<h1>Email address changed</h1>" +
                        $"<h3>Please click " +
                        $"<a href=\"{configuration.GetSection("ClientAppUrl").Value}/verify/{newEmailVerification.Token}\">here</a>" +
                        $" to confirm your new email address</h3>";
                    SendEmail(authUser.Email, "Confirm your new email", emailText);
                }

                // if user enters new password, generate new password hash and salt
                if (!string.IsNullOrEmpty(request.Password))
                {
                    CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    authUser.PasswordHash = passwordHash;
                    authUser.PasswordSalt = passwordSalt;
                }

                var profilePicturePublicId = $"profile-pictures-test2/user{authUser.Id}_profile-picture";
                if (request.DeleteProfilePicture == true) // if user wants to delete their profile pic
                {
                    var deletionParams = new DeletionParams(profilePicturePublicId)
                    {
                        ResourceType = ResourceType.Image
                    };
                    cloudinary.Destroy(deletionParams);
                    authUser.PictureURL = null;
                }
                else if (request.ProfilePicture != null) // if user wants to upload new profile pic
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

            if (authUser.Verified)
            {
                string token = CreateToken(authUser);
                return new { user = authUser, token };
            }
            else
            {
                // returns null values if users verified status changes during update
                return new { user = (User?)null, token = (string?)null };
            }
        }

        public object GetUserWithPosts(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                // Gets user posts and formats data
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
                        following = u.Following.Select(f =>
                            new
                            {
                                f.Following.Id,
                                f.Following.Username,
                                f.Following.Email,
                                f.Following.FirstName,
                                f.Following.LastName,
                                f.Following.PictureURL,
                                f.Accepted,
                            }
                        ),
                        followers = u.Followers.Select(f => new
                        {
                            f.User.Id,
                            f.User.Username,
                            f.User.Email,
                            f.User.FirstName,
                            f.User.LastName,
                            f.User.PictureURL,
                            f.Accepted,
                        }),
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
            // gets posts of users followed by current user
            var userId = GetAuthUserId();
            var userData = dbContext.Users
                .Include(u => u.Following.Where(f => f.Accepted == true)).ThenInclude(f => f.Following).ThenInclude(f => f.Posts).ThenInclude(p => p.Comments)
                .Include(u => u.Following.Where(f => f.Accepted == true)).ThenInclude(f => f.Following).ThenInclude(f => f.Posts).ThenInclude(p => p.Likes)
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
                if (foundFollow != null) // if user follows that user, remove follow
                {
                    dbContext.Follows.Remove(foundFollow);
                    dbContext.SaveChanges();
                    return "User unfollowed";
                }
                else // else, add user to following list
                {
                    var newFollow = new Follow()
                    {
                        UserId = userId,
                        FollowingId = id,
                        Accepted = false
                    };
                    dbContext.Follows.Add(newFollow);
                    dbContext.SaveChanges();
                    return "Follow request sent";
                }
            }
            throw new Exception($"User with id of {stringId} is not found");
        }

        public string RemoveFollower(string stringId)
        {
            // allows user to remove user from followers list
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

        public string AcceptFollower(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var userId = GetAuthUserId();
                if (userId == id)
                    throw new Exception("Cannot follow yourself");
                var foundUser = dbContext.Users.FirstOrDefault(u => u.Id == id);
                if (foundUser == null)
                {
                    throw new Exception($"User with id of {stringId} is not found");
                }
                var foundFollow = dbContext.Follows
                    .FirstOrDefault(f => f.UserId == id && f.FollowingId == userId);
                if (foundFollow != null)
                {
                    foundFollow.Accepted = true;
                    dbContext.SaveChanges();
                    return "Follow approved";
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

        private void SendEmail(string recipientEmail, string emailSubject, string emailText)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(configuration.GetSection("Mail:From").Value));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = emailSubject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = emailText
            };
            using var smtp = new SmtpClient();
            smtp.Connect(
                configuration.GetSection("Mail:Smtp").Value,
                int.Parse(configuration.GetSection("Mail:Port").Value),
                SecureSocketOptions.StartTls
                );
            smtp.Authenticate(
                configuration.GetSection("Mail:Username").Value,
                configuration.GetSection("Mail:Password").Value
                );
            smtp.Send(email);
            smtp.Disconnect(true);
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
