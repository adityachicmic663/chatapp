namespace backendChatApplcation.Requests
{
    public class oneToOneMessageRequest
    {
        public int senderId { get; set; }

        public int receiverId { get; set; }

        public string message { get; set; }
    }
}
