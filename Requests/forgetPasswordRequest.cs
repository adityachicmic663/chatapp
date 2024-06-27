using System.ComponentModel.DataAnnotations;

namespace backendChatApplication.Requests
{
    public class forgetPasswordRequest
    {
        [Required(ErrorMessage ="Email address is required")]
        [EmailAddress(ErrorMessage ="email address is required")]
        public string email { get; set; }
    }
}
