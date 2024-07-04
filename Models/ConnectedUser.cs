using backendChatApplication.Models;

namespace backendChatApplcation.Models
{
    public class ConnectedUser
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ConnectionId { get; set; }
        public DateTime ConnectedAt { get; set; }

        // Navigation property
        public UserModel User { get; set; }
    }
}
