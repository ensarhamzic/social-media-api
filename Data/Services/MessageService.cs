using SocialMediaAPI.Data.Models;
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
