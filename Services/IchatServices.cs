using backendChatApplcation.Models;
namespace backendChatApplcation.Services
{
    public interface IchatServices
    {
        List<chatRoomResponse>GetChatRoomsForUser(int userId,int pageNumber,int pageSize);

        chatRoomResponse CreateChatRoom(string roomName);

        void SaveGroupMessages(int senderId, int chatRoomId, string message);

        void SaveDirectMessages(int senderId, int receiverId, string message,int chatRoomId);

        void  AddUserToChatRoom(int userId, int roomId);

        Task<(List<chatResponse>, int)> GetChatAsync(int chatRoomId, int pageNumber, int pageSize);

        void RemoveUserFromChatRoom(int userId, int chatRoomId);

        Task<oneToOneResponse> SendDirectFileMessage(int senderId, int receiverId, IFormFile file);

        Task<groupChatResponse> SendFileMessage(int senderId, int chatRoomId, IFormFile file);
    }
}
