
using System.ComponentModel.DataAnnotations.Schema;


namespace backendChatApplcation.Models
{
    public class chatRoomModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int chatRoomId { get; set; }

        public string chatRoomName { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        

        public ICollection<userChatRoomModel> UserChatRooms { get; set; } = new List<userChatRoomModel>();

        public ICollection<chatMessageModel> Messages { get; set; } = new List<chatMessageModel>();

    }
}
