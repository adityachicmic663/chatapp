<<<<<<< HEAD
﻿using backendChatApplication.Models;
=======
using backendChatApplication.Models;
>>>>>>> origin/main

namespace backendChatApplcation.Models
{
    public class chatMessage
    {

        public int chatMessageId { get; set; }
        public int senderId { get; set; }
        
        public UserModel sender { get; set; }
        public string message { get; set; }
        public int chatRoomId { get; set; }

        public chatRoomModel chatRoom { get; set; }
        public DateTime SendAt { get; set; }

    }

<<<<<<< HEAD
}
=======
}
>>>>>>> origin/main
