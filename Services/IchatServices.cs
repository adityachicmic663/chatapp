
using backendChatApplcation.Models;
namespace backendChatApplcation.Services
{
    public interface IchatServices
    {
        List<chatRoomResponse> GetChatRoomsForUser(int userId);

        chatRoomResponse CreateChatRoom(string roomName, int creatorId);

        groupChatResponse SendMessage(int senderId, int chatRoomId, string message);

        oneToOneResponse SendDirectMessage(int senderId, int receiverId, string message);

        void AddUserToChatRoom(int userId, int roomId);

        void RemoveUserFromChatRoom(int userId, int chatRoomId);

        groupChatResponse SendFileMessage(int senderId, int chatRoomId, string filePath);

        oneToOneResponse SendDirectFileMessage(int senderId, int receiverId, string filePath);
    }
}
