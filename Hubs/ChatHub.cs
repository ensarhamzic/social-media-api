using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SocialMediaAPI.Hubs
{
    public class ChatHub : Hub
    {
        private IHttpContextAccessor httpContextAccessor;

        public ChatHub(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        public async Task JoinChat()
        {
            var userId = GetAuthUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

            await Clients.Group(userId.ToString())
                .SendAsync("ReceiveMessage", userId, "You have joined chat");
        }

        private int GetAuthUserId()
        {
            return int.Parse(Context.GetHttpContext().User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
