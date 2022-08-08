using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Data.Services;
using SocialMediaAPI.Data.ViewModels;

namespace SocialMediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private PostService postService;

        public PostsController(PostService postService)
        {
            this.postService = postService;
        }

        [HttpPost]
        public IActionResult CreatePost([FromBody] PostVM request)
        {
            try
            {
                var newPost = postService.CreatePost(request);
                return Created(nameof(newPost), newPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePost(string id, [FromBody] PostVM request)
        {
            try
            {
                var updatedPost = postService.UpdatePost(id, request);
                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePost(string id)
        {
            try
            {
                var response = postService.DeletePost(id);
                return Ok(new {success = response});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/likes")]
        public IActionResult LikeOrUnlikePost(string id)
        {
            try
            {
                var response = postService.LikeOrUnlikePost(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/comments")]
        public IActionResult CommentPost(string id, [FromBody] CommentVM request)
        {
            try
            {
                var comment = postService.CommentPost(id, request);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{postId}/comments/{commentId}")]
        public IActionResult DeletePostComment(string postId, string commentId)
        {
            try
            {
                var response = postService.DeletePostComment(postId, commentId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
