using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        public Chat Chat { get; set; }

        public User User { get; set; }

        public string Content { get; set; }

        public DateTime DateCreate { get; set; }

    }
}
