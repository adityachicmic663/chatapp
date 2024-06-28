
using backendChatApplcation.Models;
namespace backendChatApplcation.Services
{
    public interface IchatServices
    {
        List<chatRoomResponse> GetChatRoomsForUser(int userId);

        chatRoomResponse CreateChatRoom(string roomName, int creatorId);

        chatMessageResponse SendMessage(int senderId, int chatRoomId, string message);

        void AddUserToChatRoom(int userId, int roomId);
    }
}
