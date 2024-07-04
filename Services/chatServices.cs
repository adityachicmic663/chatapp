

using backendChatApplcation.Models;
using backendChatApplication;
using backendChatApplication.Hubs;
using backendChatApplication.Models;
using Microsoft.AspNetCore.SignalR;


namespace backendChatApplcation.Services
{
    public class chatServices:IchatServices
    {
        private readonly chatDataContext _context;
        private readonly IHubContext<chatHub> _hubContext;
        private readonly IFileService _fileService;


        public chatServices(chatDataContext dataContext,IHubContext<chatHub> hubContext,IFileService fileServices)
        {
            _context = dataContext;
            _hubContext = hubContext;
            _fileService = fileServices;
        }
        public List<chatRoomResponse> GetChatRoomsForUser(int userId)
        {
            var chatRooms = _context.UserChatRooms.Where(ucr => ucr.userId == userId).Select(ucr => new
            {
                ucr.ChatRoom,
                lastMessageTime = _context.ChatMessages
                                    .Where(cm => cm.chatRoomId == ucr.chatRoomId)
                                    .OrderByDescending(cm=>cm.sendAt)
                                    .Select(cm => (DateTime?)cm.sendAt)
                                    .FirstOrDefault()
            })
             .OrderByDescending(cr => cr.lastMessageTime)
             .ToList(); 
         
            var responseList = new List<chatRoomResponse>();

            foreach (var room in chatRooms)
            {
                var newRoom = new chatRoomResponse
                {
                    chatRoomName = room.ChatRoom.chatRoomName,
                    CreatedAt = room.ChatRoom.CreatedAt,
                    lastMessageTime=room.lastMessageTime
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
                chatRoomId = newRoom.chatRoomId,
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
                sendAt = DateTime.Now,
                filetype = null
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
                sendAt = DateTime.Now,
                filetype= null
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
        public async Task<groupChatResponse> SendFileMessage(int senderId, int chatRoomId,IFormFile file)
        {
            var filePath =await _fileService.SaveFileAsync(file);
            var filetype = _fileService.GetFileType(file.ContentType);

            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                filePath = filePath,
                filetype=filetype,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            await _hubContext.Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveFileMessage", chatRoomId, filePath, senderId, filetype);
            var response = new groupChatResponse
            {
                senderId = senderId,
                chatRoomId = chatRoomId,
                filePath = filePath,
                filetype = filetype,
                sendAt = newMessage.sendAt
            };
            return response;
        }

        public async Task<oneToOneResponse> SendDirectFileMessage(int senderId, int receiverId,IFormFile file)
        {
            var filePath = await _fileService.SaveFileAsync(file);
            var filetype = _fileService.GetFileType(file.ContentType);
            var newMessage = new chatMessageModel
            {
                senderId = senderId,
                receiverId = receiverId,
                filePath = filePath,
                filetype=filetype,
                sendAt = DateTime.Now
            };
            _context.ChatMessages.Add(newMessage);
            _context.SaveChanges();

            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveFileMessage",receiverId, filePath, senderId, filetype);

            var response = new oneToOneResponse
            {
                senderId = senderId,
                receiverId = receiverId,
                filepath = filePath,
                fileType = filetype,
                sendAt = DateTime.Now
            };
            return response;
        }



        public void  AddUserToChatRoom(int userId, int roomId)
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
            if (userChatRoom != null)
            {
                _context.UserChatRooms.Remove(userChatRoom);
                _context.SaveChanges();
            }
        }
        public List<UserResponse> GetUsersInChatRoom(int roomId)
        {
            var listofusers= _context.UserChatRooms
                .Where(uc => uc.chatRoomId == roomId)
                .Select(uc => uc.User)
                .ToList();

            var newList= new List<UserResponse>();

            foreach (var user in listofusers)
            {
                var response = new UserResponse
                {
                    userName = user.userName,
                    address = user.address,
                    gender = user.gender,
                    email = user.email,
                    phoneNumber = user.phoneNumber,
                    age = user.age

                };
                newList.Add(response);
            }
            return newList;
        }
    }
}
