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
                    gender = user.gender
                };
                responselist.Add(response);
            }
            return responselist;
        }
        public void AddUserOnline(string connectionId,string  userId)
        {
           if( _onlineUsers.TryAdd(connectionId, userId)){
                if (int.TryParse(userId, out int id))
                {
                    UpdateUserStatus(id, true);
                }
            }
        }
         
        public void RemoveUserOnline(string connectionId)
        {
            if(_onlineUsers.TryRemove(connectionId, out string userId))
            {
                if (int.TryParse(userId, out int id))
                {
                    UpdateUserStatus(id, false);
                }
            }
        }

        public List<string> GetOnlineUsers()
        {
            return _onlineUsers.Values.Distinct().ToList();
        }
        public string GetConnectionId(int userId)
        {
            return _onlineUsers.FirstOrDefault(x => x.Value == userId.ToString()).Key;
        }

        private void UpdateUserStatus(int userId,bool isOnline)
        {
            var user = _context.users.FirstOrDefault(x => x.userId == userId);
            if (user != null)
            {
                user.isOnline = isOnline;
                _context.SaveChanges();
            }
        }
    }
}
