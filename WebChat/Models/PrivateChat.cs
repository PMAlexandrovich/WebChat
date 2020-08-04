using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class PrivateChat
    {
        public int Id { get; set; }

        public User User { get; set; }

        public User Interlocutor { get; set; }

        public bool IsVisible { get; set; }

        public int CountOfUnreadMessages { get; set; }

        public DateTime LastMessageWas { get; set; }
    }
}
