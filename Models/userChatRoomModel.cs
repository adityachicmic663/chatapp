
using backendChatApplication.Models;
using Microsoft.Data.SqlClient.DataClassification;


namespace backendChatApplcation.Models
{
    public class userChatRoomModel
    {
        public int userId { get; set; }
        public int chatRoomId { get; set; }
        public DateTime joinedAt { get; set; } = DateTime.Now;

        public UserModel User { get; set; }
        public chatRoomModel ChatRoom { get; set; }
    }
}
