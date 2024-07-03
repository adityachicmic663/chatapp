
using backendChatApplcation.Models;
using backendChatApplcation.Services;
using backendChatApplication;
using backendChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace backendChatApplcation.Hubs
{
    [Authorize]
    public  class chatHub : Hub
    {
        private readonly IchatServices _chatServices;
        private readonly IUserServices _userServices;
        private readonly IFileService _fileService;
        private readonly chatDataContext _context;


        
        public chatHub(IchatServices chatService, IUserServices userService, IFileService fileService)
        {
            _chatServices = chatService;
            _userServices = userService;
            _fileService = fileService;
           
        }


        public async Task AddUserToGroup(int groupId, string message)
        {
            if (_context.ChatRooms.Any(x => x.chatRoomId == groupId))
            {
                var user = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
                _chatServices.AddUserToChatRoom(user.userId, groupId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
                await Clients.Group(groupId.ToString()).SendAsync("UserJoined", message);
            }
        }


        public async Task RemoveUserFromGroup(int groupId)
        {
                var user = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
                  _chatServices.RemoveUserFromChatRoom(user.userId, groupId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
                await Clients.Group(groupId.ToString()).SendAsync("UserLeftGroup");
        }
        public async Task ShareFileToGroup(int groupId,string filePath)
        {
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);

            if (sender != null && _context.ChatRooms.Any(x => x.chatRoomId == groupId))
            {
                _chatServices.SendFileMessage(sender.userId, groupId, filePath);
                await Clients.Group(groupId.ToString()).SendAsync("ReceivedFileMessage", filePath);
            }
        }

        public async Task ShareFileToUser( string userEmail, string filePath)
        {
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
            var receiver = _context.users.FirstOrDefault(x => x.email == userEmail);

            if (sender != null && receiver != null)
            {
                _chatServices.SendDirectFileMessage(sender.userId, receiver.userId, filePath);
                await Clients.User(userEmail).SendAsync("ReceivedFileMessage", sender.userId, filePath);
            }

        }

        public async Task SendMessageToGroup(string message, int groupId)
        {
            string userEmail = Context.User.Identity.Name;
            var user=_context.users.FirstOrDefault(x=>x.email == userEmail);
            _chatServices.SendMessage(user.userId, groupId, message);

            await Clients.Group(groupId.ToString()).SendAsync("ReceivedGroupMessage",message);
        }

        public async Task SendPrivateMessage(string userEmail,string message)
        {
            var receiver = _context.users.FirstOrDefault(c => c.email == userEmail);
            var sender = _context.users.FirstOrDefault(x => x.email == Context.User.Identity.Name);
            _chatServices.SendDirectMessage(sender.userId, receiver.userId, message);
          Clients.User(userEmail).SendAsync("receivedUserMessage",message);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);
          
            string userId = Context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userServices.AddUserOnline(Context.ConnectionId, userId);
                await Clients.All.SendAsync("UserConnected", $"{Context.ConnectionId}{userId} has joined the chat");
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
        }


        public override async Task OnDisconnectedAsync(Exception ex)
        {

            Console.WriteLine("Client disconnected " + Context.ConnectionId);
            string userId = Context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userServices.RemoveUserOnline(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                await Clients.All.SendAsync("user disconnected", userId);
            }

            await base.OnDisconnectedAsync(ex);
        }

        public async Task<List<UserWithStatus>> UsersWithStatus()
        {

            string userEmail = Context.User.Identity.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == userEmail);
            if (currentUser != null)
            {
                var usersWithStatus = _userServices.GetUsersWithStatus(currentUser.userId);
                return usersWithStatus;
            }
            return new List<UserWithStatus>();
        }

    }
}
