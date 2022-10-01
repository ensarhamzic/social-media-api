using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Data.Services;

namespace SocialMediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private MessageService messageService;

        public MessagesController(MessageService messageService)
        {
            this.messageService = messageService;
        }

        [HttpGet("{id}")]
        public IActionResult GetMessagesWithUser(string id)
        {
            try
            {
                var response = messageService.GetMessagesWithUser(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}
