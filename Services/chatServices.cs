

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
            var chatResponses = new List<chatRoomResponse>();

            var groupChats = _context.UserChatRooms
                .Where(ucr => ucr.userId == userId)
                .Select(ucr => new
                {
                    ucr.ChatRoom,
                    lastMessageTime = _context.ChatMessages
                                        .Where(cm => cm.chatRoomId == ucr.chatRoomId)
                                        .OrderByDescending(cm => cm.sendAt)
                                        .Select(cm => (DateTime?)cm.sendAt)
                                        .FirstOrDefault()
                })
                .ToList();

          
            var directChats = _context.ChatMessages
                .Where(cm => cm.senderId == userId || cm.receiverId == userId)
                .GroupBy(cm => cm.senderId == userId ? cm.receiverId : cm.senderId)
                .Select(g => new
                {
                    ChatRoom = (chatRoomModel)null,
                    DirectUser = _context.users.FirstOrDefault(u => u.userId == g.Key),
                    firstMessage = g.OrderBy(cm => cm.sendAt).FirstOrDefault(), 
                    lastMessageTime = g.Max(cm => cm.sendAt) 
                })
                .ToList();

           
            foreach (var group in groupChats)
            {
                if (group.ChatRoom != null)
                {
                    var newGroupChat = new chatRoomResponse
                    {
                        chatRoomId = group.ChatRoom.chatRoomId,
                        chatRoomName = group.ChatRoom.chatRoomName,
                        CreatedAt = group.ChatRoom.CreatedAt,
                        lastMessageTime = group.lastMessageTime
                    };
                    chatResponses.Add(newGroupChat);
                }
            }

            foreach (var direct in directChats)
            {
                if (direct.DirectUser != null && direct.firstMessage != null)
                {
                    var newDirectChat = new chatRoomResponse
                    {
                        chatRoomId = direct.firstMessage.chatMessageId,
                        chatRoomName = direct.DirectUser.userName,
                        CreatedAt = direct.firstMessage.sendAt,
                        lastMessageTime = direct.lastMessageTime
                    };
                    chatResponses.Add(newDirectChat);
                }
            }

          
            return chatResponses.OrderByDescending(cr => cr.lastMessageTime).ToList();

        }

        public chatRoomResponse CreateChatRoom(int roomId,string roomName, int creatorId)
        {
            var newRoom = new chatRoomModel
            {
                chatRoomId = roomId,
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

        public void  SendMessage(int senderId, int chatRoomId, string message)
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
        }
        public void SendDirectMessage(int senderId, int receiverId, string message)
        {
            try
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

             
            }catch(Exception ex) 
            {
                Console.WriteLine($"Exception sending direct message: {ex.Message}");
                throw; 
            }
            
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
