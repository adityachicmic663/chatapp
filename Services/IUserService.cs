
using backendChatApplcation.Models;
namespace backendChatApplcation.Services
{
    public interface IUserServices
    {

        List<UserResponse> SearchUser(string searchkey);

        List<string> GetOnlineUsers();

        void AddUserOnline(string connectionId,string userId);

        void RemoveUserOnline(string connectionId);

        List<UserWithStatus> GetUsersWithStatus(int userId);
    }
}

