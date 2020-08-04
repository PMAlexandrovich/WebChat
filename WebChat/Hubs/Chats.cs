using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebChat.Data;
using WebChat.Models;

namespace WebChat.Hubs
{
    [Authorize]
    public class Chats : Hub
    {
        UserManager<User> UserManager;
        ApplicationDbContext dbContext;

        public Chats(UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.dbContext = dbContext;
        }

        public async Task Send(string message, string name, int chatId)
        {
            Chat chat = dbContext.Chats.FirstOrDefault(u => u.Id == chatId);
            var userNames = dbContext.Parties.Where(p => p.Chat == chat).Select(u => u.User.UserName).ToList();
            User user = await UserManager.FindByNameAsync(Context.User.Identity.Name);
            dbContext.Messages.Add(new Message { Chat = chat, Content = message, User = user, DateCreate = DateTime.Now });
            await Clients.Users(userNames).SendAsync("Receive", new object[1] { new object[] { message, name } });
            dbContext.SaveChanges();
        }

        public async Task OnConnect(int chatId)
        {
            var messages = dbContext.Messages
                 .Where(x => x.Chat.Id == chatId)
                 .OrderBy(x => x.DateCreate)
                 .Select(x => new object[] { x.Content, x.User.UserName, x.DateCreate.ToString("yyyy-MM-ddTHH:mm:ss.ffffff") })
                 .ToList();
            await Clients.Caller.SendAsync("_loadChat", messages);
        }
    }
}
