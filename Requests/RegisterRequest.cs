using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace backendChatApplication.Requests
{
    public class RegisterRequest
    {

        [Required(ErrorMessage = "username is required")]
        public string userName { get; set; }


        [Required(ErrorMessage = "email is required")]
        [EmailAddress(ErrorMessage ="not a valid email")]
        public string email { get; set; }


        [Required(ErrorMessage = "password is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain each of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string password { get; set; }

        public string? profilePicturePath { get; set; }

        [Required]
        public string gender { get; set; }
        [Required]
        public string address {  get; set; }
        [Required]
        public int age { get; set; }
        [Required]
        public long phoneNumber { get; set; }

    }
}
