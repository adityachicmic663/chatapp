namespace backendChatApplcation.Models
{
    public class groupChatResponse
    {
        public int senderId { get; set; }

        public int? chatRoomId { get; set; }

        public string message { get; set; }
        public string? filePath { get; set; }

        public string? filetype { get; set; }

        public DateTime sendAt { get; set; }
    }
}
