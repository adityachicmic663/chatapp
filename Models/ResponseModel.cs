namespace backendChatApplication.Models
{
    public class ResponseModel
    {
        public int statusCode { get; set; }

        public object message { get; set; }

        public object data { get; set; }

        public bool isSuccess { get; set; }

    }
}
