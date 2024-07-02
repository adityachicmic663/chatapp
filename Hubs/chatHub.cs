
using backendChatApplcation.Services;
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
        private readonly ILogger<chatHub> _logger;
        



        public chatHub(IchatServices chatService, IUserServices userService, IFileService fileService, ILogger<chatHub> logger)
        {
            _chatServices = chatService;
            _userServices = userService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task AddUserToGroup(string groupName, string message, string userName)
        {
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoined", message, userName);
        }


        public async Task RemoveUserFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup");
        }
        public async Task ShareFileToGroup(string groupName, int senderId, IFormFile file)
        {
            var filePath = await _fileService.SaveFileAsync(file);
            await Clients.Group(groupName).SendAsync("ReceivedFileMessage", senderId, filePath);
        }

        public async Task ShareFileToUser(int senderId, string user, IFormFile file)
        {
            var filePath = await _fileService.SaveFileAsync(file);

            await Clients.Client(user).SendAsync("ReceivedFileMessage", senderId, filePath);

        }

        public async Task SendMessageToGroup(string user, string message, string groupName)
        {

            await Clients.Group(groupName).SendAsync("ReceivedMessage", user, message);
           
        }

        public async Task SendMessage(string message)
        {
            try
            {
                await Clients.All.SendAsync("ReceivedMessage", message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendMessage method: {ex.Message}");
                throw;
            }
        }
        public async Task SendPrivateMessage(string user, string message)
        {
            await Clients.Users(user).SendAsync("ReceivedMessage", message);
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

        public async Task<List<string>> onlineUsers()
        {
            var list = _userServices.GetOnlineUsers();
            foreach(var onlineusers in list)
            {
                Console.WriteLine($"Online users: {onlineusers}");
            }
            return list;
        }

    }
}
