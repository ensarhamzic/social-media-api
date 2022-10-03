using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data.Models;
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
            // get all messages that user sent or received
            var messages = dbContext.Messages
                .Where(m => m.FromUserId == userId || m.ToUserId == userId)
                .Include(m => m.FromUser).Include(m => m.ToUser);

            // group by sender id (remove duplicates)
            var fromMessages = messages.GroupBy(m => m.FromUserId)
                .Select(m => m.FirstOrDefault());

            // group by receiver id (remove duplicates)
            var toMessages = messages.GroupBy(m => m.ToUserId)
                .Select(m => m.FirstOrDefault());

            // extracts just users but only unique ones
            List<User> chatUsers = new List<User>();
            foreach(var msg in fromMessages)
            {
                if (msg == null) break;
                chatUsers.Add(msg.FromUser);
            }

            foreach(var msg in toMessages)
            {
                if (msg == null) break;
                if (chatUsers.Any(c => c.Id == msg.ToUser.Id)) continue;
                chatUsers.Add(msg.ToUser);
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
