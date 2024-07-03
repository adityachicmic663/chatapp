namespace backendChatApplcation.Models
{
    public class UserWithStatus
    {
        public int userId { get; set; }

        public string userName { get; set; }

        public string email { get; set; }

        public bool isOnline {  get; set; }
    }
}
