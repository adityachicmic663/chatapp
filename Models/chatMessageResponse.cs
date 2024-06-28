namespace backendChatApplcation.Models
{
    public class chatMessageResponse
    {
        public int senderId { get; set; }

        public string message { get; set; }

        public DateTime sendAt { get; set; }
    }
}
