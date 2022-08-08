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
                UserId = int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid))
            };
            dbContext.Posts.Add(post);
            dbContext.SaveChanges();
            return post;
        }
    }
}
