using System.ComponentModel.DataAnnotations;

namespace backendChatApplication.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "email is required")]
        [EmailAddress(ErrorMessage = "not a valid email")]
        public string email {  get; set; }

        [Required(ErrorMessage = "password is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain each of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string password { get; set; }

    }
}
