using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Data.Services;
using SocialMediaAPI.Data.ViewModels;

namespace SocialMediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private PostService postService;

        public PostsController(PostService postService)
        {
            this.postService = postService;
        }

        [HttpPost, Authorize]
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
    }
}
