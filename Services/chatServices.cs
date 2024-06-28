
using backendChatApplcation.Models;
using backendChatApplcation.Requests;
using backendChatApplication;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq.Expressions;

namespace backendChatApplcation.Services
{
    public class chatServices:IchatServices
    {
        private readonly chatDataContext _context;

        public chatServices(chatDataContext dataContext)
        {
            _context = dataContext;
        }
        public List<chatRoomResponse> GetChatRoomsForUser(int userId)
        {
            var userExists=_context.users.Any(u=>u.userId == userId);
            if (!userExists)
            {
                return new List<chatRoomResponse>();
            }
            
              var listofChats  = _context.UserChatRooms.Where(ucr => ucr.userId == userId).Select(ucr=>ucr.ChatRoom).ToList();
            var chatlistResponse=new List<chatRoomResponse>();

            foreach(var chatRoom in listofChats)
            {
                var response = new chatRoomResponse
                {
                    chatRoomName = chatRoom.chatRoomName,
                    CreatedAt = chatRoom.CreatedAt
                };
               chatlistResponse.Add(response);
            }
           return chatlistResponse;
        }

        public chatRoomResponse CreateChatRoom(string roomName, int creatorId)
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
                joinedAt = DateTime.Now,
                ChatRoom=newRoom
            };

            _context.UserChatRooms.Add(userChatRoom);

            _context.SaveChanges();

            var response = new chatRoomResponse
            {
                chatRoomName = newRoom.chatRoomName,
                CreatedAt = newRoom.CreatedAt
            };

            return response;
        }
     
        public chatMessageResponse SendMessage(int senderId, int chatRoomId, string message)
        {
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                message = message,
                SendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();
            var response = new chatMessageResponse
            {
                senderId = senderId,
                message = message,
                sendAt = newMessage.SendAt
            };
            return response;
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
