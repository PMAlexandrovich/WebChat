using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebChat.Data;
using WebChat.Models;

namespace WebChat.Hubs
{
    [Authorize]
    public class ContactsHub:Hub
    {
        UserManager<User> UserManager;
        ApplicationDbContext dbContext;

        public ContactsHub(UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.dbContext = dbContext;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
