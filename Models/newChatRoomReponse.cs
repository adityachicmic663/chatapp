namespace backendChatApplcation.Models
{
    public class newChatRoomReponse
    {
        public int chatRoomId { get; set; }
        public string chatRoomName { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? lastMessageTime { get; set; }

        public List<UserWithStatus> list { get; set; }
    }
}
