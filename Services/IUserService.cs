
using backendChatApplcation.Models;
using backendChatApplication;

using backendChatApplication.Models;

namespace backendChatApplcation.Services
{
    public interface IUserServices
    {

        List<UserResponse> SearchUser(string searchkey);

        void AddUserOnline(string connectionId, string userId);

        void RemoveUserOnline(string connectionId);

        List<string> GetOnlineUsers();
    }
}

