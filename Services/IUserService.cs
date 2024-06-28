using backendChatApplication;
using backendChatApplication.Models;

namespace backendChatApplcation.Services
{
    public interface IUserServices
    {

        List<UserModel> SearchUser(string searchkey);

        void UpdateUserStatus(int userId, bool isOnline);
        
    }
}
