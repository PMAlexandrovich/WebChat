using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebChat.Models;

namespace WebChat.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageStatus> MessageStatuses { get; set; }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<RequestAddContact> Requests { get; set; }

        //public DbSet<PrivateChat> PrivateChats { get; set; }
        //public DbSet<PrivateParty> PrivateParties { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<PrivateChat> PrivateChatList { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
