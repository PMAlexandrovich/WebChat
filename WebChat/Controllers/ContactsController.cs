using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebChat.Data;
using WebChat.Models;
using WebChat.Hubs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebChat.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Route("api/[controller]/[action]")]
    public class ContactsController : Controller
    {
        ApplicationDbContext DbContext;
        UserManager<User> UserManager;
        IHubContext<ContactsHub> HubContext;

        public ContactsController(ApplicationDbContext dbContext, UserManager<User> userManager, IHubContext<ContactsHub> hubContext)
        {
            DbContext = dbContext;
            UserManager = userManager;
            HubContext = hubContext;
        }

        // GET: api/<controller>
        [HttpGet]
        public JsonResult Get()
        {
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var Contacts = DbContext.Contacts.Where(c => c.Owner == user).Select(c => new { UserId = c.User.Id, Name = c.User.UserName });
            
            return Json(Contacts.AsEnumerable());
        }
        [HttpGet]
        public JsonResult GetInboxList()
        {
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var Contacts = DbContext.Requests.Where(c => c.Receiver == user).Select(c => new { UserId = c.Sender.Id, Name = c.Sender.UserName });

            return Json(Contacts.AsEnumerable());
        }
        [HttpGet]
        public JsonResult GetOutboxList()
        {
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var Contacts = DbContext.Requests.Where(c => c.Sender == user).Select(c => new { UserId = c.Receiver.Id, Name = c.Receiver.UserName, IsCanceled = c.Canceled});

            return Json(Contacts.AsEnumerable());
        }

        // POST api/<controller>
        [HttpPost]
        public StatusCodeResult AddContact()
        {

            string receiverName = Request.Form["username"];
            User receiver = UserManager.FindByNameAsync(receiverName).Result;
            User sender = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var alreadyExist = DbContext.Requests.FirstOrDefault((x) => x.Receiver == receiver & x.Sender == sender);

            if (alreadyExist != null)
                return StatusCode(500);

            if (receiver == null || receiverName == User.Identity.Name)
                return StatusCode(500);

            DbContext.Requests.Add(new RequestAddContact() { Sender = sender, Receiver = receiver, Canceled = false });
            DbContext.SaveChanges();
            HubContext.Clients.User(receiverName).SendAsync("ReceivedContact", User.Identity.Name);
            return StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult RejectContact()
        {

            string senderName = Request.Form["username"];
            User sender = UserManager.FindByNameAsync(senderName).Result;
            User receiver = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var request = DbContext.Requests.FirstOrDefault((x) => x.Receiver == receiver & x.Sender == sender);

            if (request == null)
                return StatusCode(500);

            if (receiver == null || sender == null)
                return StatusCode(500);
            request.Canceled = true;
            DbContext.Requests.Update(request);
            DbContext.SaveChanges();
            HubContext.Clients.User(senderName).SendAsync("RejectedContact", User.Identity.Name);
            return StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult CancelContact()
        {

            string receiverName = Request.Form["username"];
            User receiver = UserManager.FindByNameAsync(receiverName).Result;
            User sender = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var request = DbContext.Requests.FirstOrDefault((x) => x.Receiver == receiver & x.Sender == sender);

            if (request == null)
                return StatusCode(500);

            if (receiver == null || receiverName == User.Identity.Name)
                return StatusCode(500);

            DbContext.Requests.Remove(request);
            DbContext.SaveChanges();
            HubContext.Clients.User(receiverName).SendAsync("CanceledContact", User.Identity.Name);
            return StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult RequestAgainContact()
        {

            string receiverName = Request.Form["username"];
            User sender = UserManager.FindByNameAsync(User.Identity.Name).Result;
            User receiver = UserManager.FindByNameAsync(receiverName).Result;
            var request = DbContext.Requests.FirstOrDefault((x) => x.Receiver == receiver && x.Sender == sender);

            if (request == null)
                return StatusCode(500);

            if (receiver == null || sender == null)
                return StatusCode(500);
            request.Canceled = false;
            DbContext.Requests.Update(request);
            DbContext.SaveChanges();
            HubContext.Clients.User(receiverName).SendAsync("ReceivedContact", User.Identity.Name);
            //HubContext.Clients.Clients(senderName).SendAsync("RequestAgainContact", User.Identity.Name);
            return StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult AcceptContact()
        {

            string senderName = Request.Form["username"];
            User sender = UserManager.FindByNameAsync(senderName).Result;
            User receiver = UserManager.FindByNameAsync(User.Identity.Name).Result;
            var request = DbContext.Requests.FirstOrDefault((x) => x.Receiver == receiver & x.Sender == sender);

            if (request == null)
                return StatusCode(500);

            if (receiver == null || senderName == User.Identity.Name)
                return StatusCode(500);

            DbContext.Requests.Remove(request);
            DbContext.Contacts.Add(new Contact() { Owner = sender, User = receiver });
            DbContext.Contacts.Add(new Contact() { Owner = receiver, User = sender });
            
            DbContext.SaveChanges();
            HubContext.Clients.User(senderName).SendAsync("AcceptContact", User.Identity.Name);
            //HubContext.Clients.Clients(User.Identity.Name).SendAsync("AcceptedContact", senderName);
            return StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult DeleteContact()
        {

            string otherName = Request.Form["username"];
            User other = UserManager.FindByNameAsync(otherName).Result;
            User user = UserManager.FindByNameAsync(User.Identity.Name).Result;

            if (other == null || otherName == User.Identity.Name)
                return StatusCode(500);

            var contact1 = DbContext.Contacts.FirstOrDefault((x) => x.Owner == other && x.User == user);
            var contact2 = DbContext.Contacts.FirstOrDefault((x) => x.Owner == user && x.User == other);
            DbContext.Contacts.Remove(contact1);
            DbContext.Contacts.Remove(contact2);
            DbContext.SaveChanges();
            HubContext.Clients.User(otherName).SendAsync("DeletedContact", User.Identity.Name);
            //HubContext.Clients.Clients(User.Identity.Name).SendAsync("DeletedContact", otherName);
            return StatusCode(200);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //var contact = DbContext.Contacts.FirstOrDefault(c => c.Owner == )
            //DbContext.Contacts.Remove()
        }
    }
}
