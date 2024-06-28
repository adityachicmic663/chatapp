namespace backendChatApplcation.Requests
{
    public class sendMessageRequest
    {
        public int senderId { get; set; }

        public int chatRoomId { get; set; }

        public string message { get; set; }
    }
}
