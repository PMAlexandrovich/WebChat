using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Claims;
using WebChat.Models;
using Microsoft.AspNetCore.Identity;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebChat.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private readonly UserManager<User> userManager;

        public AccountController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost("/reg")]
        public async Task<ActionResult> Register()
        {
            var username = Request.Form["username"];
            var password = Request.Form["password"];

            var existUser = await userManager.FindByNameAsync(username);
            if (existUser != null)
            {
                ModelState.AddModelError("Name", "Пользователь с таким именем уже существует");
                return BadRequest(ModelState);
            }
            User user = new User { UserName = username };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                ModelState.AddModelError("Password", "Пароль должен содержать минимум 6 символов латинского алфавита и минимум 1 цифру");
            if (ModelState.IsValid)
                return Ok();
            return BadRequest(ModelState);
        }

        [HttpPost("/token")]
        public async Task<ActionResult> Token()
        {
            var username = Request.Form["username"];
            var password = Request.Form["password"];

            var identity = GetIdentity(username, password);
            if(identity == null)
            {
                ModelState.AddModelError("User", "Неверное имя пользователя или пароль");
                return BadRequest(ModelState);
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromDays(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            //Response.ContentType = "application/json";
            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            return Json(response);
        }

        private ClaimsIdentity GetIdentity(string username, string pasword)
        {
            User user = userManager.FindByNameAsync(username).Result;
            bool flag = false;
            if (user != null)
            {
                flag = userManager.CheckPasswordAsync(user, pasword).Result;
            }
            

            if(flag == true)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType,user.UserName),
                    //new Claim(ClaimsIdentity.DefaultRoleClaimType,user.Role)
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            return null;
        }
    }
}
