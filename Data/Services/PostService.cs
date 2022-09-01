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
                UserId = GetAuthUserId(),
                Date = DateTime.Now
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

        public Comment CommentPost(string postStringId, CommentVM request)
        {
            CheckId(postStringId, out int postId, out bool isValid);
            if (isValid)
            {
                var foundPost = dbContext.Posts.FirstOrDefault(p => p.Id == postId);
                if(foundPost != null)
                {
                    var newComment = new Comment()
                    {
                        Text = request.Text,
                        PostId = postId,
                        UserId = GetAuthUserId(),
                        Date = DateTime.Now
                    };
                    dbContext.Comments.Add(newComment);
                    dbContext.SaveChanges();
                    return newComment;
                }
            }
            throw new Exception($"Post with id of {postStringId} does not exist");
        }

        public object DeletePostComment(string postStringId, string commentStringId)
        {
            CheckId(postStringId, out int postId, out bool isPostIdValid);
            CheckId(commentStringId, out int commentId, out bool isCommentIdValid);
            if(isPostIdValid && isCommentIdValid)
            {
                var foundPost = dbContext.Posts.FirstOrDefault(p => p.Id == postId);
                if(foundPost != null)
                {
                    var foundComment = dbContext.Comments.FirstOrDefault(c => c.Id == commentId);
                    if(foundComment != null)
                    {
                        if((UserHasPermission(foundComment.UserId) || UserHasPermission(foundPost.UserId))
                            && foundComment.PostId == postId)
                        {
                            dbContext.Comments.Remove(foundComment);
                            dbContext.SaveChanges();
                            return new { success = "successfully deleted comment!" };
                        }
                    }
                }
            }
            throw new Exception($"Cannot find that comment or that post, or you do not have rights to delete this comment");
        }

        public object LikeOrUnlikePost(string postStringId)
        {
            CheckId(postStringId, out int postId, out bool isValid);
            if (isValid)
            {
                var userId = GetAuthUserId();
                var foundLike = dbContext.Likes.FirstOrDefault
                    (l => l.UserId == userId && l.PostId == postId);
                if (foundLike != null)
                {
                    dbContext.Likes.Remove(foundLike);
                    dbContext.SaveChanges();
                    return new { success = "successfully unliked post" };
                }
                else
                {
                    var newLike = new Like()
                    {
                        UserId = userId,
                        PostId = postId,
                    };
                    dbContext.Likes.Add(newLike);
                    dbContext.SaveChanges();
                    return new { success = "successfully liked post" };
                }
            }
            throw new Exception($"Post with id of {postStringId} does not exist");
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

        /// <summary>
        /// Gets User data from json web token
        /// </summary>
        /// <returns>Id of user</returns>

        private int GetAuthUserId()
        {
            return int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
