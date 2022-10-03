using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace SocialMediaAPI.Data.Services
{
    public class MessageService
    {
        private AppDbContext dbContext;
        private IHttpContextAccessor httpContextAccessor;

        public MessageService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        public List<Message> GetMessagesWithUser(string stringId)
        {
            CheckId(stringId, out int id, out bool isValid);
            var userId = GetAuthUserId();
            if (isValid)
            {
                var messages = dbContext.Messages
                    .Where(m => (m.FromUserId == userId && m.ToUserId == id)
                    || (m.FromUserId == id && m.ToUserId == userId)).OrderBy(m => m.TimeSent).ToList();
                return messages;
            }
            throw new Exception($"User with id of {stringId} is not found");
        }

        public object GetChats()
        {
            var userId = GetAuthUserId();
            var user = dbContext.Users
                .Include(u => u.ReceivedMessages).ThenInclude(m => m.FromUser)
                .FirstOrDefault(u => u.Id == userId);
            var receivedMessages = user.ReceivedMessages;
            List<User> allChatUsers = new List<User>();
            foreach(var msg in receivedMessages)
                allChatUsers.Add(msg.FromUser);
            List<UserChat> chatUsers = new List<UserChat>();
            foreach(var usr in allChatUsers)
            {
                if (chatUsers.Any(u => u.Id == usr.Id)) continue;
                var isNewChat = receivedMessages.Any(m => m.FromUserId == usr.Id && !m.Seen);
                var newUserChat = new UserChat() {
                    Id = usr.Id,
                    FirstName = usr.FirstName,
                    LastName = usr.LastName,
                    Email = usr.Email,
                    Username = usr.Username,
                    PictureURL = usr.PictureURL,
                    NewChat = isNewChat,
                };
                chatUsers.Add(newUserChat);
            }


            return chatUsers;
        }

        private void CheckId(string stringId, out int id, out bool isValid)
        {
            bool valid = int.TryParse(stringId, out int convertedId);
            isValid = valid;
            id = convertedId;
        }

        private int GetAuthUserId()
        {
            return int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}
