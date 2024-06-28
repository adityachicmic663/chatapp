using backendChatApplcation.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace backendChatApplication.Models
{
    public class UserModel
    {
        [Required]
        [Key]
        public int  userId { get; set; }

        public string userName { get; set; }

        public string role { get; set; } 

        public string email { get; set; }
        public string password { get; set; }

        public long phoneNumber { get; set; }

        public bool emailConfirmed { get; set; }

        public string address { get; set; } = "india";

        public string FirstLanguage { get; set; } 

        public int age { get; set; }

        public string gender { get; set; } = "Male";

        public string otpToken { get; set; }

        public DateTime? OtpTokenExpiry { get; set; }
        public string profilePicturePath { get; set; }

        public bool isOnline { get; set; } = false;

        public ICollection<userChatRoom> UserChatRooms { get; set; } = new List<userChatRoom>();

        public ICollection<chatMessage> SentMessages { get; set; } = new List<chatMessage>();

    }
}
