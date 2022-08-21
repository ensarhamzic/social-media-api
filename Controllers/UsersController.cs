using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Data.Services;
using SocialMediaAPI.Data.ViewModels;

namespace SocialMediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserService userService;
        public UsersController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterVM request)
        {
            try
            {
                var response = userService.Register(request);
                return Created(nameof(response), response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginVM request)
        {
            try
            {
                var response = userService.Login(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut, Authorize]
        public IActionResult UpdateUser([FromForm] UserUpdateVM request)
        {
            try
            {
                var response = userService.UpdateUserAsync(request);
                return Ok(response.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message });
            }
        } 

        [HttpPost("verify"), Authorize]
        public IActionResult VerifyToken()
        {
            try
            {
                var response = userService.GetAuthUserData();
                return Ok(new { user = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/follow"), Authorize]
        public IActionResult FollowOrUnfollowUser(string id)
        {
            try
            {
                var message = userService.FollowOrUnfollowUser(id);
                return Ok(new { success = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/posts"), Authorize]
        public IActionResult GetUserWithPosts(string id)
        {
            try
            {
                var userWithPosts = userService.GetUserWithPosts(id);
                return Ok(userWithPosts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("feed"), Authorize]
        public IActionResult GetUserFeed()
        {
            try
            {
                var userFeed = userService.GetUserFeed();
                return Ok(userFeed);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
