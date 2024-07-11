

using backendChatApplcation.Models;
using backendChatApplication;
using backendChatApplication.Hubs;
using backendChatApplication.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;


namespace backendChatApplcation.Services
{
    public class chatServices:IchatServices
    {
        private readonly chatDataContext _context;
        private readonly IHubContext<chatHub> _hubContext;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserServices _userServices;



        public chatServices(chatDataContext dataContext,IHubContext<chatHub> hubContext,IFileService fileServices,IHttpContextAccessor httpContextAccessor,IUserServices userServices)
        {
            _context = dataContext;
            _hubContext = hubContext;
            _fileService = fileServices;
            _httpContextAccessor = httpContextAccessor;
            _userServices = userServices;
        }
        public List<chatRoomResponse> GetChatRoomsForUser(int userId,int pageNumber,int pageSize)
        {
            var chatResponses = new List<chatRoomResponse>();

            var allChats = _context.ChatMessages
                .Where(cm => cm.senderId == userId || cm.receiverId == userId ||
                             _context.UserChatRooms.Any(uc => uc.userId == userId && uc.chatRoomId == cm.chatRoomId))
                .GroupBy(cm => cm.chatRoomId)
                .Select(g => new
                {
                    ChatRoom = _context.ChatRooms.FirstOrDefault(cr => cr.chatRoomId == g.Key),
                    lastMessageTime = g.Max(cm => cm.sendAt)
                })
                .ToList();

          
            foreach (var chat in allChats)
            {
                if (chat.ChatRoom != null)
                {
                    var newChat = new chatRoomResponse
                    {
                        chatRoomId = chat.ChatRoom.chatRoomId,
                        chatRoomName = chat.ChatRoom.chatRoomName,
                        CreatedAt = chat.ChatRoom.CreatedAt,
                        lastMessageTime = chat.lastMessageTime
                    };
                    chatResponses.Add(newChat);
                }
            }

            var responseList = chatResponses.OrderByDescending(cr => cr.lastMessageTime).ToList();

            var totalmessage=responseList.Count;

            var messages = responseList.Skip((pageNumber-1)*pageSize).Take(pageSize).ToList();

            var list = _userServices.GetUsersWithStatus(userId);

            return messages;
        }



        public chatRoomResponse CreateChatRoom(string roomName)
        {
            var newRoom = new chatRoomModel
            {
                chatRoomName = roomName,
                CreatedAt = DateTime.Now
            };
            _context.ChatRooms.Add(newRoom);
            _context.SaveChanges();

            var userEmail = _httpContextAccessor?.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = _context.users.FirstOrDefault(x => x.email == userEmail);
            if (user != null)
            {
                AddUserToChatRoom(user.userId, newRoom.chatRoomId);
            }
            var userConnectionId = _context.UserConnections
                .Where(c => c.UserId == user.userId)
                .OrderByDescending(c => c.ConnectedAt)
                .Select(c => c.ConnectionId)
                .FirstOrDefault();

            if (userConnectionId != null)
            {
                _hubContext.Groups.AddToGroupAsync(userConnectionId, newRoom.chatRoomId.ToString());
            }

            var response = new chatRoomResponse
            {
                chatRoomId = newRoom.chatRoomId,
                chatRoomName = newRoom.chatRoomName,
                CreatedAt = newRoom.CreatedAt
            };
            return response;
        }




        public void SaveGroupMessages(int senderId, int chatRoomId, string message)
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
        public void SaveDirectMessages(int senderId, int receiverId, string message,int chatRoomId)
        {
            try
            {
                var newMessage = new chatMessageModel
                {
                    senderId = senderId,
                    receiverId = receiverId,
                    chatRoomId=chatRoomId,
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

        public async Task<(List<chatResponse>, int)> GetChatAsync(int chatRoomId, int Noofmessages, int msgNo)
        {
            try
            {
                var userEmail = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                var user = await _context.users.FirstOrDefaultAsync(x => x.email == userEmail);

                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                var query = _context.ChatMessages.AsQueryable();

                var isMemberOfChatRoom = _context.UserChatRooms
                .Any(ur => ur.chatRoomId == chatRoomId && ur.userId == user.userId);

                if (!isMemberOfChatRoom)
                {
                    throw new Exception("User is not a member of this chat room.");
                }

                query = query.Where(x => x.chatRoomId == chatRoomId);
                                         

                query = query.OrderByDescending(x => x.sendAt);

                var totalMessages = await query.CountAsync();

                var messages = await query.Skip(Noofmessages).Take(msgNo).ToListAsync();

                var responseList = messages.Select(message1 => new chatResponse
                {
                    senderId = message1.senderId,
                    message = message1.message,
                    sendAt = message1.sendAt
                }).ToList();

                return (responseList, totalMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during fetching chat messages: {ex.Message}");
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
