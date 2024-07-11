using backendChatApplcation.Models;
using backendChatApplcation.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;


namespace backendChatApplication.Hubs
{
    [Authorize]
    public class chatHub : Hub
    {
        private readonly IchatServices _chatServices;
        private readonly IUserServices _userServices;
        private readonly IFileService _fileService;
        private readonly chatDataContext _context;

        public chatHub(IchatServices chatServices, IUserServices userServices, IFileService fileService, chatDataContext context)
        {
            _chatServices = chatServices;
            _userServices = userServices;
            _fileService = fileService;
            _context = context;
        }

        public async Task AddUserToGroup(string userEmail, int groupId)
        {
            try
            {
                var member = _context.users.FirstOrDefault(x => x.email == userEmail);
                var creatorEmail = Context.User.Identity.Name;
                var creator = _context.users.FirstOrDefault(x => x.email == creatorEmail);

                if (creator == null)
                {
                    await Clients.Caller.SendAsync("UserNotFound", "User not found.");
                    return;
                }

                var chatRoomExists = _context.ChatRooms.Any(x => x.chatRoomId == groupId);

                if (!chatRoomExists)
                {
                    await Clients.Caller.SendAsync("RoomNotAvailable", "The chat room is not available.");
                    return;
                }

                if (!_context.UserChatRooms.Any(uc => uc.userId == member.userId && uc.chatRoomId == groupId))
                {
                    _chatServices.AddUserToChatRoom(member.userId, groupId);
                }
                var room=_context.ChatRooms.FirstOrDefault(x=>x.chatRoomId == groupId);

                var chatroom = new chatRoomResponse
                {
                    chatRoomId = groupId,
                    chatRoomName = room.chatRoomName,
                    CreatedAt = room.CreatedAt
                };

                var member1 = new UserResponse
                {
                    userName = member.userName,
                    email = member.email,
                    address = member.address,
                    age = member.age,
                    gender = member.gender
                };
             
                var userConnectionId = _context.UserConnections
                    .Where(c => c.UserId == member.userId)
                    .OrderByDescending(c => c.ConnectedAt) 
                    .Select(c => c.ConnectionId)
                    .FirstOrDefault();

                if (userConnectionId != null)
                {
                    await Groups.AddToGroupAsync(userConnectionId, groupId.ToString());
                }

                await Clients.Group(groupId.ToString()).SendAsync("UserJoined",member1,chatroom);
            }
            catch (DbUpdateException ex)
            {
                await Clients.Caller.SendAsync("Error", "An error occurred while updating the database. See server logs for details.");
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "An unexpected error occurred. See server logs for details.");
                Console.WriteLine($"Exception: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        public async Task RemoveUserFromGroup(int groupId)
        {
            var user = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
            if (user == null)
            {
                await Clients.Caller.SendAsync("UserNotFound", "User not found.");
                return;
            }
            var group=_context.ChatRooms.FirstOrDefault(x=>x.chatRoomId == groupId);
            var user1 = new UserResponse
            {
                userName = user.userName,
                address = user.address,
                age = user.age,
                email = user.email,
                gender = user.gender,
                phoneNumber = user.phoneNumber
            };

            var group1 = new chatRoomResponse
            {
                chatRoomId = groupId,
                chatRoomName = group.chatRoomName,
                CreatedAt = group.CreatedAt
            };

            _chatServices.RemoveUserFromChatRoom(user.userId, groupId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
            await Clients.Group(groupId.ToString()).SendAsync("UserLeftGroup",user1,group1);
        }


        public async Task SendMessageToGroup(string message, int groupId)
        {
            try
            {
                var userEmail = Context.User.Identity.Name;
                var user = _context.users.FirstOrDefault(x => x.email == userEmail);
                if (user == null)
                {
                    await Clients.Caller.SendAsync("Error", "user not found");
                    return;
                }
                var messageResponse = new chatMessageResponse
                {
                    senderId = user.userId,
                    message = message,
                    sendAt = DateTime.Now,
                };
                var group = _context.ChatRooms.FirstOrDefault(x => x.chatRoomId == groupId);

                var group1 = new chatRoomResponse
                {
                    chatRoomId = groupId,
                    chatRoomName = group.chatRoomName,
                    CreatedAt = group.CreatedAt
                };
                var user1 = new UserResponse
                {
                    userName = user.userName,
                    address = user.address,
                    age = user.age,
                    email = user.email,
                    gender = user.gender
                };
               
                if (user != null && _context.UserChatRooms.Any(x => x.chatRoomId == groupId && x.userId == user.userId))
                {
                    _chatServices.SaveGroupMessages(user.userId, groupId, message);

                    await Clients.Group(groupId.ToString()).SendAsync("ReceivedGroupMessage",messageResponse,user1,group1);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this group.");
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
                await Clients.Caller.SendAsync("Error", "An error occurred while saving changes to the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.InnerException?.Message ?? ex.Message}");
                await Clients.Caller.SendAsync("Error", "An unexpected error occurred.");
            }

        }

        public async Task SendPrivateMessage(string receiverEmail, string message)
        {
            try
            {
                var senderEmail= Context.User.Identity.Name;
                var receiver = _context.users.FirstOrDefault(c => c.email == receiverEmail);
                var sender = _context.users.FirstOrDefault(x => x.email == senderEmail);

                if (sender != null && receiver != null)
                {

                    var chatRoom = _context.ChatRooms.Include(x => x.UserChatRooms).FirstOrDefault(x => x.UserChatRooms.Count == 2
                                                                                       && x.UserChatRooms.Any(x => x.userId == sender.userId)
                                                                                        && x.UserChatRooms.Any(x => x.userId == receiver.userId));

                    if (chatRoom == null)
                    {
                        var roomName = $"{sender.userName}-{receiver.userName}";
                        var room = _chatServices.CreateChatRoom(roomName);
                        chatRoom = _context.ChatRooms.FirstOrDefault(x => x.chatRoomId == room.chatRoomId);
                        _chatServices.AddUserToChatRoom(receiver.userId, chatRoom.chatRoomId);
                    }

                    var messageResponse = new chatMessageResponse
                    {
                        senderId = sender.userId,
                        message = message,
                        sendAt = DateTime.Now
                    };

                    var roomReponse = new chatRoomResponse
                    {
                        chatRoomId = chatRoom.chatRoomId,
                        chatRoomName = chatRoom.chatRoomName,
                        CreatedAt = chatRoom.CreatedAt

                    };
                    var user1 = new UserResponse
                    {
                        userName = sender.userName,
                        address = sender.address,
                        age = sender.age,
                        email = sender.email,
                        gender = sender.gender,
                        phoneNumber = sender.phoneNumber
                    };


                    _chatServices.SaveDirectMessages(sender.userId, receiver.userId, message, chatRoom.chatRoomId);
                    var receiverUserId = receiver.userId.ToString();
                    await Clients.User(receiverUserId).SendAsync("ReceivedUserMessage",messageResponse,user1,roomReponse);
                }
                else
                {
                    Console.WriteLine("Sender or receiver not found.");
                    await Clients.Caller.SendAsync("Error", "Sender or receiver not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.InnerException?.Message ?? ex.Message}");
                await Clients.Caller.SendAsync("Error", "An unexpected error occurred.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);

            var userEmail = Context.User.Identity.Name;
            var user = _context.users.FirstOrDefault(x => x.email == userEmail);

            if (user != null)
            {

                var userGroups = _context.UserChatRooms
                    .Where(uc => uc.userId == user.userId)
                    .Select(uc => uc.chatRoomId)
                    .ToList();

                foreach (var groupId in userGroups)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
                }

                var connectedUser = new ConnectedUser
                {
                    UserId = user.userId,
                    UserName = user.userName,
                    Email = user.email,
                    ConnectionId = Context.ConnectionId,
                    ConnectedAt = DateTime.UtcNow
                };

                _context.UserConnections.Add(connectedUser);
                await _context.SaveChangesAsync();

                _userServices.AddUserOnline(Context.ConnectionId, user.userId.ToString());
                await Clients.Caller.SendAsync("UserConnected", $"user:{user.userName}");

                var onlineUsers = _userServices.GetOnlineUsers();
                await Clients.Caller.SendAsync("OnlineUsers",onlineUsers );

                var usersWithStatus = _userServices.GetUsersWithStatus(user.userId);
                await Clients.Caller.SendAsync("UsersWithStatus",usersWithStatus);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "User not found.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userEmail = Context.User.Identity.Name;
            var user = _context.users.FirstOrDefault(x => x.email == userEmail);

            if (user != null)
            {
                _userServices.RemoveUserOnline(Context.ConnectionId);

                var connectedUser = _context.UserConnections.FirstOrDefault(uc => uc.ConnectionId == Context.ConnectionId);
                if (connectedUser != null)
                {
                    _context.UserConnections.Remove(connectedUser);
                    await _context.SaveChangesAsync();
                }

                await Clients.All.SendAsync("UserDisconnected", $"user :{user.userName}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
