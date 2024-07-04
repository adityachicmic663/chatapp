namespace backendChatApplcation.Models
{
    public class oneToOneResponse
    {
        public int senderId { get; set; }

        public int? receiverId { get; set; }

        public string message { get; set; }
        public string? filepath { get; set; }

        public string? fileType { get; set; }
        public DateTime sendAt { get; set; }
    }
}
