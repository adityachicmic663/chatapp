namespace backendChatApplcation.Models
{
    public class chatResponse
    {
        public int senderId { get; set; }

        public string message { get; set; }

        public DateTime? sendAt { get; set; }
    }
}
