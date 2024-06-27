namespace backendChatApplication.Models
{
    public class ResponseModel
    {
        public int statusCode { get; set; }

        public string message { get; set; }

        public object data { get; set; }

        public bool isSuccess { get; set; }

    }
}
