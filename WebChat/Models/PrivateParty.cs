using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class PrivateParty
    {
        public User User { get; set; }

        public User Interlocutor { get; set; }

        public PrivateChat PrivateChat { get; set; }
    }
}
