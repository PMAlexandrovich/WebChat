using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public User Owner { get; set; }

        public User User { get; set; }
    }
}
