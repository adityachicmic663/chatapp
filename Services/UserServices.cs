using backendChatApplication;
using backendChatApplication.Models;

namespace backendChatApplcation.Services
{
    public class UserServices:IUserServices
    {
        private readonly chatDataContext _context;

        public UserServices(chatDataContext context)
        {
           _context = context;
        }

        public List<UserModel> SearchUser(string searchkey)
        {
            return _context.users.Where(x=>x.userName.Contains(searchkey)).ToList();
        }

        public void UpdateUserStatus(int userId, bool isOnline)
        {
            var user = _context.users.Find(userId);
            if (user != null)
            {
                user.isOnline = isOnline;
                _context.SaveChanges();
            }
        }
    }
}
