using System.ComponentModel.DataAnnotations;

namespace backendChatApplication.Requests
{
    public class PasswordChangeRequest
    {
        [Required(ErrorMessage ="Please enter the password for whom you to change password")]
        public string email { get; set; }

        [Required(ErrorMessage ="enter the new password")]
        public string newPassword { get; set; }
    }
}
