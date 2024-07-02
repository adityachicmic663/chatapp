
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
            var chatRooms = _context.UserChatRooms.Where(ucr => ucr.userId == userId).Select(ucr => ucr.ChatRoom).ToList();

            var responseList = new List<chatRoomResponse>();

            foreach (var room in chatRooms)
            {
                var newRoom = new chatRoomResponse
                {
                    chatRoomName = room.chatRoomName,
                    CreatedAt = room.CreatedAt
                };
                responseList.Add(newRoom);
            }
            return responseList;
        }

        public chatRoomResponse CreateChatRoom(string roomName, int creatorId)
        {
            var newRoom = new chatRoomModel
            {
                chatRoomName = roomName,
                CreatedAt = DateTime.Now
            };
            _context.ChatRooms.Add(newRoom);

            _context.SaveChanges();

            var userChatRoom = new userChatRoomModel
            {
                userId = creatorId,
                chatRoomId = newRoom.chatRoomId,
                joinedAt = DateTime.Now
            };

            _context.UserChatRooms.Add(userChatRoom);

            _context.SaveChanges();
            var response = new chatRoomResponse
            {
                chatRoomName = roomName,
                CreatedAt = newRoom.CreatedAt
            };

            return response;
        }

        public groupChatResponse SendMessage(int senderId, int chatRoomId, string message)
        {
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                message = message,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            var response = new groupChatResponse
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                message = message,
                sendAt = newMessage.sendAt
            };

            return response;
        }
        public oneToOneResponse SendDirectMessage(int senderId, int receiverId, string message)
        {
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                receiverId = receiverId,
                message = message,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            var response = new oneToOneResponse
            {
                senderId = senderId,
                receiverId = receiverId,
                message = message,
                sendAt = newMessage.sendAt
            };

            return response;
        }
        public groupChatResponse SendFileMessage(int senderId, int chatRoomId, string filePath)
        {
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                filePath = filePath,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            var response = new groupChatResponse
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                filePath = filePath,
                sendAt = newMessage.sendAt
            };
            return response;
        }

        public oneToOneResponse SendDirectFileMessage(int senderId, int receiverId, string filePath)
        {
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                receiverId = receiverId,
                filePath = filePath,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            var response = new oneToOneResponse
            {
                senderId = senderId,
                receiverId = receiverId,
                filepath = filePath,
                sendAt = DateTime.Now
            };
            return response;
        }

        public void AddUserToChatRoom(int userId, int roomId)
        {
            var userChatRoom = new userChatRoomModel
            {
                userId = userId,
                chatRoomId = roomId,
                joinedAt = DateTime.Now
            };

            _context.UserChatRooms.Add(userChatRoom);
            _context.SaveChanges();
        }

        public void RemoveUserFromChatRoom(int userId, int chatRoomId)
        {
            var userChatRoom = _context.UserChatRooms.FirstOrDefault(x => x.userId == userId && x.chatRoomId == chatRoomId);
            _context.UserChatRooms.Remove(userChatRoom);
            _context.SaveChanges();

        }
    }
}
