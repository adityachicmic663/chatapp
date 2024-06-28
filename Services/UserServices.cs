using backendChatApplcation.Models;
using backendChatApplication;
using backendChatApplication.Models;
using System.Collections.Concurrent;

namespace backendChatApplcation.Services
{
    public class UserServices:IUserServices
    {
        private readonly chatDataContext _context;
        private static ConcurrentDictionary<string, string> _onlineUsers = new ConcurrentDictionary<string, string>();


        public UserServices(chatDataContext context)
        {
           _context = context;
        }

        public List<UserResponse> SearchUser(string searchkey)
        {
            var listOfUsers= _context.users.Where(x=>x.userName.Contains(searchkey)|| x.email.Contains(searchkey)).ToList();
            var responselist=new List<UserResponse>();
            foreach(var user in listOfUsers)
            {
                var response = new UserResponse()
                {
                    userName = user.userName,
                    email = user.email,
                    address = user.address,
                    phoneNumber = user.phoneNumber,
                    age = user.age,
                    isOnline = false
                };
                responselist.Add(response);
            }
            return responselist;
        }
        public void AddUserOnline(string connectionId,string userId)
        {
            _onlineUsers.TryAdd(connectionId, userId);
        }
         
        public void RemoveUserOnline(string connectionId)
        {
            _onlineUsers.TryRemove(connectionId, out _);
        }

        public List<string> GetOnlineUsers()
        {
            return _onlineUsers.Values.Distinct().ToList();
        }
    }
}
