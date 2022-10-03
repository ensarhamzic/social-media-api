using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Hubs.ViewModels;
using System.Security.Claims;

namespace SocialMediaAPI.Hubs
{
    public class ChatHub : Hub
    {
        private IHttpContextAccessor httpContextAccessor;
        private AppDbContext dbContext;

        public ChatHub(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }

        [Authorize]
        public async Task JoinChat()
        {
            var userId = GetAuthUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        }

        [Authorize]
        public async Task SendMessage(NewMessageVM msg)
        {
            var userId = GetAuthUserId();
            var user = await dbContext.Users.Where(u => u.Id == userId).Select(u => new {u.Id, u.FirstName, u.LastName, u.PictureURL, u.Username, u.Email}).FirstOrDefaultAsync();
            var newMessage = new Message()
            {
                Text = msg.Message,
                FromUserId = userId,
                ToUserId = msg.To,
                TimeSent = DateTime.Now,
                Seen = false,
            };

            await dbContext.AddAsync(newMessage);
            await dbContext.SaveChangesAsync();

            await Clients.Group(msg.To.ToString())
                .SendAsync("ReceiveMessage", user, newMessage);
            await Clients.Group(userId.ToString())
                .SendAsync("ReceiveMessage", user, newMessage);
        }

        [Authorize]
        public async Task SeenMessages(int chatUserId)
        {
            var userId = GetAuthUserId();
            var msgs = dbContext.Messages
                .Where(m => m.FromUserId == chatUserId
                && m.ToUserId == userId
                && !m.Seen);
            foreach(var msg in msgs)
                msg.Seen = true;
            await dbContext.SaveChangesAsync();
        }

        private int GetAuthUserId()
        {
            return int.Parse(Context.GetHttpContext().User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
