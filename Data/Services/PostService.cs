using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;
using System.Security.Claims;

namespace SocialMediaAPI.Data.Services
{
    public class PostService
    {
        private AppDbContext dbContext;
        private IHttpContextAccessor httpContextAccessor;
        public PostService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Post CreatePost(PostVM request)
        {
            var post = new Post()
            {
                Text = request.Text,
                UserId = GetAuthUserId()
            };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();
            return post;
        }

        

        public Post UpdatePost(string stringId, PostVM request)
        {
            string errorMessage = $"Post with id of {stringId} is not found!";
            CheckId(stringId, out int id, out bool isValid);
            if(isValid)
            {
                var foundPost = dbContext.Posts.FirstOrDefault(p => p.Id == id);
                if(foundPost != null)
                {
                    if (UserHasPermission(foundPost.UserId))
                    {
                        foundPost.Text = request.Text;
                        dbContext.SaveChanges();
                        return foundPost;
                    }
                    else
                        errorMessage = "You don't have permission to update this post!";
                }
            }
            throw new Exception(errorMessage);
        }

        public string DeletePost(string stringId)
        {
            string errorMessage = $"Post with id of {stringId} is not found!";
            CheckId(stringId, out int id, out bool isValid);
            if (isValid)
            {
                var foundPost = dbContext.Posts.FirstOrDefault(p => p.Id == id);
                if (foundPost != null)
                {
                    if (UserHasPermission(foundPost.UserId))
                    {
                        dbContext.Posts.Remove(foundPost);
                        dbContext.SaveChanges();
                        return "Post successfully deleted";
                    }
                    else
                        errorMessage = "You don't have permission to delete this post!";
                }
            }
            throw new Exception(errorMessage);
        }

        private void CheckId(string stringId, out int id, out bool isValid)
        {
            bool valid = int.TryParse(stringId, out int convertedId);
            isValid = valid;
            id = convertedId;
        }

        private bool UserHasPermission(int userId)
        {
            return userId == GetAuthUserId();
        }

        private int GetAuthUserId()
        {
            return int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
