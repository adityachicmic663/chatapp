namespace backendChatApplcation.Models
{
    public class chatRoomResponse
    { 
        public int chatRoomId { get; set;}
        public string chatRoomName { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? lastMessageTime { get; set; }
    }
}
