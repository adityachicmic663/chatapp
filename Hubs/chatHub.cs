using backendChatApplcation.Models;
using backendChatApplcation.Services;
using backendChatApplication.Models;
using backendChatApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task AddUserToGroup(int groupId, string message)
        {
            try
            {
                var user = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
                if (user == null)
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

                if (!_context.UserChatRooms.Any(uc => uc.userId == user.userId && uc.chatRoomId == groupId))
                {
                    _chatServices.AddUserToChatRoom(user.userId, groupId);
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
                await Clients.Group(groupId.ToString()).SendAsync("UserJoined", message);
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

            _chatServices.RemoveUserFromChatRoom(user.userId, groupId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
            await Clients.Group(groupId.ToString()).SendAsync("UserLeftGroup");
        }

        public async Task ShareFileToGroup(int groupId, string filePath)
        {
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);

            if (sender != null && _context.ChatRooms.Any(x => x.chatRoomId == groupId))
            {
                await Clients.Group(groupId.ToString()).SendAsync("ReceivedFileMessage", filePath);
            }
        }

        public async Task ShareFileToUser(string userEmail, string filePath)
        {
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
            var receiver = _context.users.FirstOrDefault(x => x.email == userEmail);

            if (sender != null && receiver != null)
            {
                await Clients.User(userEmail).SendAsync("ReceivedFileMessage", sender.userId, filePath);
            }
        }

        public async Task SendMessageToGroup(string message, int groupId)
        {
            try
            {
                var userEmail = Context.User.Identity.Name;
                var user = _context.users.FirstOrDefault(x => x.email == userEmail);

                if (user != null && _context.UserChatRooms.Any(x => x.chatRoomId == groupId && x.userId == user.userId))
                {
                    _chatServices.SendMessage(user.userId, groupId, message);
                }

                await Clients.Group(groupId.ToString()).SendAsync("ReceivedGroupMessage", message);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
                await Clients.Caller.SendAsync("Error", "An error occurred while saving changes to the database.");
            }
        }

        public async Task SendPrivateMessage(string userEmail, string message)
        {
            var receiver = _context.users.FirstOrDefault(c => c.email == userEmail);
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);

            if (sender != null && receiver != null)
            {
                _chatServices.SendDirectMessage(sender.userId, receiver.userId, message);
                await Clients.User(userEmail).SendAsync("ReceivedUserMessage", message);
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
                await Clients.Caller.SendAsync("UserConnected", user.userName);

                var onlineUsers = _userServices.GetOnlineUsers();
                await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);

                var usersWithStatus = _userServices.GetUsersWithStatus(user.userId);
                await Clients.Caller.SendAsync("UsersWithStatus", usersWithStatus);
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

                await Clients.All.SendAsync("UserDisconnected", user.userName);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
