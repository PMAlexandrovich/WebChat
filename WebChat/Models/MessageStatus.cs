using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class MessageStatus
    {
        public int Id { get; set; }

        public Message Message { get; set; }

        public bool IsRead { get; set; }
    }
}
