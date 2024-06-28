using backendChatApplcation.Models;

namespace backendChatApplcation.Services
{
    public interface IchatServices
    {
        List<chatRoomModel> GetChatRoomsForUser(int userId);

        chatRoomModel CreateChatRoom(string roomName, int creatorId);

        chatMessage SendMessage(int senderId, int chatRoomId, string message);

        void AddUserToChatRoom(int userId, int roomId);
    }
}
