using backendChatApplcation.Models;
namespace backendChatApplcation.Services
{
    public interface IchatServices
    {
        List<chatRoomResponse> GetChatRoomsForUser(int userId);

        chatRoomResponse CreateChatRoom(int roomId,string roomName, int creatorId);

       void SendMessage(int senderId, int chatRoomId, string message);

        void SendDirectMessage(int senderId, int receiverId, string message);

        void  AddUserToChatRoom(int userId, int roomId);

        void RemoveUserFromChatRoom(int userId, int chatRoomId);

        Task<oneToOneResponse> SendDirectFileMessage(int senderId, int receiverId, IFormFile file);

        Task<groupChatResponse> SendFileMessage(int senderId, int chatRoomId, IFormFile file);
    }
}
