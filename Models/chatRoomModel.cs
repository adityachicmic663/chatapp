using System.ComponentModel.DataAnnotations.Schema;

namespace backendChatApplcation.Models
{
    public class chatRoomModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int chatRoomId { get; set; }

        public string chatRoomName { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public ICollection<userChatRoom> UserChatRooms { get; set; } = new List<userChatRoom>();

        public ICollection<chatMessage> Messages { get; set; } = new List<chatMessage>();

    }
}
