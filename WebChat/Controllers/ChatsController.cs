using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebChat.Data;
using WebChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebChat.Controllers
{
    [Route("api/[controller]")]
    public class ChatsController : Controller
    {
        ApplicationDbContext DbContext;
        UserManager<User> UserManager;

        public ChatsController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            DbContext = dbContext;
            UserManager = userManager;
        }


        [HttpGet]
        [Authorize]
        //Отправляет список чатов
        public JsonResult Get()
        {
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var Parties = DbContext.Parties.Where(x => x.User == user).Select(x => new{x.Id, x.Chat.Name,IsCreator = x.Chat.Creator == user});
            if (Parties == null)
                return null;
            return Json(Parties.AsEnumerable());
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "All Bad";
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize]
        //Создать чат
        public void Post()
        {
            var chatName = Request.Form["chat_name"];
            User Creator = UserManager.FindByNameAsync(User.Identity.Name).Result;
            Chat newChat = new Chat { Creator = Creator, Name = chatName };
            DbContext.Chats.Add(newChat);
            DbContext.Parties.Add(new Party { Chat = newChat, User = Creator });
            DbContext.SaveChanges();
        }

        // PUT api/<controller>/5
        [HttpPut]
        public void Put([FromBody]string contact_name)
        {

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
