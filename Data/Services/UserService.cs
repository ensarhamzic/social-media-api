using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.ViewModels;

namespace SocialMediaAPI.Data.Services
{
    public class UserService
    {
        private AppDbContext dbContext;
        public UserService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

    }
}
