using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class PrivateMessage
    {
        public int PrivateMessageId { get; set; }

        //public PrivateChat PrivateChat { get; set; }

        public User Sender { get; set; }

        public User Receiver { get; set; }

        public string Content { get; set; }

        public DateTime DateCreate { get; set; }

        public bool WasRead { get; set; }
    }
}
