using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebChat.Data;
using WebChat.Hubs;
using WebChat.Models;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebChat.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class PrivateChatController : Controller
    {
        ApplicationDbContext DbContext;
        UserManager<User> UserManager;
        IHubContext<PrivateChatHub> HubContext;

        public PrivateChatController(ApplicationDbContext dbContext, UserManager<User> userManager, IHubContext<PrivateChatHub> hubContext)
        {
            DbContext = dbContext;
            UserManager = userManager;
            HubContext = hubContext;
        }

        [HttpGet("{receiverName}")]
        public JsonResult GetMessagesWith(string receiverName)
        {
            User receiver = UserManager.FindByNameAsync(receiverName).Result;
            User sender = UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (receiver == null || sender == null)
                return null;
            var messages = DbContext.PrivateMessages.Where((x) => (x.Sender == sender && x.Receiver == receiver) || (x.Sender == receiver && x.Receiver == sender)).Select((x) => new { Sender = x.Sender.UserName, Receiver = x.Receiver.UserName, Content = x.Content, DateCreate = x.DateCreate });
            return Json(messages.AsEnumerable());
        }

        // POST api/<controller>
        [HttpPost]
        public ActionResult SendMessage()
        {
            string receiverName = Request.Form["username"];
            string message = Request.Form["message"];
            DateTime dateCreate = DateTime.Parse(Request.Form["dateCreate"]);
            User receiver = UserManager.FindByNameAsync(receiverName).Result;
            User sender = UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (receiver == null || sender == null)
                return StatusCode(500);

            DbContext.PrivateMessages.Add(new PrivateMessage() { Sender = sender, Receiver = receiver, Content = message, DateCreate = dateCreate});
            var privateChat = DbContext.PrivateChatList.FirstOrDefault((x) => x.User == receiver && x.Interlocutor == sender);
            if(privateChat == null)
            {
                DbContext.PrivateChatList.Add(new PrivateChat() { User = receiver, Interlocutor = sender, IsVisible = true, CountOfUnreadMessages = 1, LastMessageWas = dateCreate });
                DbContext.PrivateChatList.Add(new PrivateChat() { User = sender, Interlocutor = receiver, IsVisible = true, CountOfUnreadMessages = 0, LastMessageWas = dateCreate });
            }
            else
            {
                privateChat.CountOfUnreadMessages++;
                privateChat.IsVisible = true;
                privateChat.LastMessageWas = dateCreate;
                DbContext.PrivateChatList.Update(privateChat);
            }
            DbContext.SaveChanges();
            HubContext.Clients.User(receiverName).SendAsync("ReceivedMessage", User.Identity.Name, message,dateCreate);
            return StatusCode(200);
        }

        [HttpPost("{senderName}")]
        public ActionResult AllMessagesWasReadWith(string senderName)
        {
            User interlocator = UserManager.FindByNameAsync(senderName).Result;
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (interlocator == null || user == null)
                return StatusCode(500);
            var privatechat = DbContext.PrivateChatList.FirstOrDefault((x) => x.User == user && x.Interlocutor == interlocator);
            privatechat.CountOfUnreadMessages = 0;
            DbContext.PrivateChatList.Update(privatechat);
            DbContext.SaveChanges();

            return StatusCode(200);
        }

        [HttpGet]
        public JsonResult GetAllVisiblePrivateChats()
        {
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (user == null)
                return null;
            var privatechats = DbContext.PrivateChatList.Where((x) => x.User == user && x.IsVisible == true).OrderByDescending((x)=> x.LastMessageWas).Select((x)=> new { Name = x.Interlocutor.UserName, CountOfUnreadMessages = x.CountOfUnreadMessages});

            var col = privatechats.AsEnumerable();
            return Json(col);
        }
    }
}
