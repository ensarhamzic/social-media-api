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
                var token = userService.Register(request);
                var response = new { token };
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
                var token = userService.Login(request);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify"), Authorize]
        public IActionResult VerifyToken()
        {
            return Ok(new { success = "Token is valid" });
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
        [HttpGet("test")]
        public IActionResult UserTest()
        {
            return Ok(userService.UserTest());
        }
    }
}
