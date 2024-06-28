<<<<<<< HEAD
﻿using backendChatApplcation.Services;
=======
using backendChatApplcation.Services;
>>>>>>> origin/main
using Microsoft.AspNetCore.SignalR;

namespace backendChatApplcation.Hubs
{
    public class chatHub:Hub
    {
        private readonly chatServices _chatService;

        public chatHub(chatServices chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(int senderId,int chatRoomId,string message)
        {
            _chatService.SendMessage(senderId, chatRoomId, message);

            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceivedMessage", senderId, message); 
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.User.Identity.Name;  // assuming userId is stored in Claims
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);  //Add user to a group(chat room) based on userId

            await Clients.Others.SendAsync("UserConnected", userId);  //Notify clients of user status change
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userId = Context.User.Identity.Name;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);// remove user from groups and notify other of disconnection
            await Clients.Others.SendAsync("user disconnected", userId);

            await base.OnDisconnectedAsync(ex);
        }

    }
}
