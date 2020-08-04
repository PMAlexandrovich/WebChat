using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class Party
    {
        public int Id { get; set; }

        public Chat Chat { get; set; }

        public User User { get; set; }
    }
}
