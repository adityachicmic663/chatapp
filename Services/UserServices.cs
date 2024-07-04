
using backendChatApplcation.Models;
using backendChatApplication;
using backendChatApplication.Hubs;
using backendChatApplication.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace backendChatApplcation.Services
{
    public class UserServices : IUserServices
    {
        private readonly chatDataContext _context;
        private readonly IHubContext<chatHub> _hubContext;
        private static ConcurrentDictionary<string, string> _onlineUsers = new ConcurrentDictionary<string, string>();


        public UserServices(chatDataContext context)
        {
            _context = context;
        }

        public List<UserResponse> SearchUser(string searchkey)
        {
            var listOfUsers = _context.users.Where(x => x.userName.Contains(searchkey) || x.email.Contains(searchkey)).ToList();
            var responselist = new List<UserResponse>();
            foreach (var user in listOfUsers)
            {
                var response = new UserResponse()
                {
                    userName = user.userName,
                    email = user.email,
                    address = user.address,
                    phoneNumber = user.phoneNumber,
                    age = user.age,
                    gender = user.gender,

                };
                responselist.Add(response);
            }
            return responselist;
        }
        public void AddUserOnline(string connectionId, string userId)
        {
            if (_onlineUsers.TryAdd(connectionId, userId))
            {
                if (int.TryParse(userId, out int parsedUserId))
                {
                    UpdateUserStatus(parsedUserId, true);
                }
            }
        }

        public void RemoveUserOnline(string connectionId)
        {
            if (_onlineUsers.TryRemove(connectionId, out string userId))
            {
                if (int.TryParse(userId, out int parsedUserId))
                {
                    UpdateUserStatus(parsedUserId, false);
                }
            }
        }

        public List<string> GetOnlineUsers()
        {
            return _onlineUsers.Values.Distinct().ToList();
        }

        private void UpdateUserStatus(int userId, bool isOnline)
        {
            var user = _context.users.FirstOrDefault(x => x.userId == userId);
            if (user != null)
            {
                user.isOnline = isOnline;
                _context.SaveChanges();

                var previousInteractions = GetInteractedUsers(userId);

                foreach (var otherUserId in previousInteractions)
                {
                    NotifyUserStatusChange(otherUserId, userId, isOnline);
                }
            }
        }

        public List<UserWithStatus> GetUsersWithStatus(int userId)
        {
            var interactedUserIds = GetInteractedUsers(userId);
            var onlineUserIds = _onlineUsers.Values.Select(int.Parse).ToList();

            var usersWithStatus = _context.users
                .Where(u => interactedUserIds.Contains(u.userId))
                .Select(user => new UserWithStatus
                {
                    userId = user.userId,
                    userName = user.userName,
                    email = user.email,
                    isOnline = onlineUserIds.Contains(user.userId)
                })
                .ToList();

            return usersWithStatus;
        }
        private List<int> GetInteractedUsers(int userId)
        {
            var directMessages = _context.ChatMessages
                .Where(m => m.senderId == userId || m.receiverId == userId)
                .Select(m => m.senderId == userId ? m.receiverId : m.senderId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var groupMessages = _context.UserChatRooms
                .Where(uc => uc.userId == userId)
                .SelectMany(uc => uc.ChatRoom.UserChatRooms)
                .Where(uc => uc.userId != userId)
                .Select(uc => uc.userId)
                .Distinct()
                .ToList();

            return directMessages.Union(groupMessages).Distinct().ToList();
        }
        private void NotifyUserStatusChange(int targetUserId, int changedUserId, bool isOnline)
        {
            var targetUserConnections = _onlineUsers
                .Where(kvp => kvp.Value == targetUserId.ToString())
                .Select(kvp => kvp.Key)
                .ToList();

            if (targetUserConnections.Any())
            {
                _hubContext.Clients.Clients(targetUserConnections).SendAsync("UserStatusChanged", changedUserId, isOnline);
            }
        }

    }
}
