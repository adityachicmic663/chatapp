
using backendChatApplcation.Services;
using Microsoft.AspNetCore.SignalR;

namespace backendChatApplcation.Hubs
{
    public class chatHub : Hub
    {
        private readonly IchatServices _chatService;
        private readonly IUserServices _userService;


        public chatHub(IchatServices chatService,IUserServices userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        public async Task SendMessage(int senderId,int chatRoomId,string message)
        {
            _chatService.SendMessage(senderId, chatRoomId, message);

            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceivedMessage", senderId, message); 
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.User.Identity.Name;  // assuming userId is stored in Claims
            if(!string.IsNullOrEmpty(userId) )
            {
                _userService.AddUserOnline(Context.ConnectionId, userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);  //Add user to a group(chat room) based on userId
                await Clients.Others.SendAsync("UserConnected", userId);  //Notify clients of user status change
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userId = Context.User.Identity.Name;
            if(!string.IsNullOrEmpty(userId) )
            {
                _userService.RemoveUserOnline(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);  // remove user from groups and notify other of disconnection
                await Clients.Others.SendAsync("user disconnected", userId);
            }

            await base.OnDisconnectedAsync(ex);
        }

    }
}
