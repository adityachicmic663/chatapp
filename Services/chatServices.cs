using backendChatApplcation.Models;
using backendChatApplication;

namespace backendChatApplcation.Services
{
    public class chatServices:IchatServices
    {
        private readonly chatDataContext _context;

        public chatServices(chatDataContext dataContext)
        {
            _context = dataContext;
        }
        public List<chatRoomModel> GetChatRoomsForUser(int userId)
        {
            return _context.UserChatRooms.Where(ucr => ucr.userId == userId).Select(ucr=>ucr.ChatRoom).ToList();
        }

        public chatRoomModel CreateChatRoom(string roomName, int creatorId)
        {
            var newRoom = new chatRoomModel
            {
                chatRoomName = roomName,
                CreatedAt = DateTime.Now
            };
            _context.ChatRooms.Add(newRoom);

            var userChatRoom = new userChatRoom
            {
                userId = creatorId,
                chatRoomId = newRoom.chatRoomId,
                joinedAt = DateTime.Now
            };

            _context.UserChatRooms.Add(userChatRoom);

            _context.SaveChanges();

            return newRoom;
        }
     
        public chatMessage SendMessage(int senderId, int chatRoomId, string message)
        {
            var newMessage = new chatMessage
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                message = message,
                SendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            return newMessage;
        }

        public void AddUserToChatRoom(int userId, int roomId)
        {
            var userChatRoom = new userChatRoom
            {
                userId = userId,
                chatRoomId = roomId,
                joinedAt = DateTime.Now
            };

            _context.UserChatRooms.Add(userChatRoom);
            _context.SaveChanges();
        }
    }
}
