
using backendChatApplication.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendChatApplcation.Models
{
    public class chatMessageModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int chatMessageId { get; set; }
        public int senderId { get; set; }

        public UserModel sender { get; set; }

        public int? receiverId { get; set; }

        public UserModel receiver { get; set; }
        public string? message { get; set; }
        public int? chatRoomId { get; set; }

        public chatRoomModel chatRoom { get; set; }
        public string? filePath { get; set; }

        public string? filetype { get; set; }

        public DateTime sendAt { get; set; }


    }

}


