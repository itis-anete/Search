using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Search.DataBase.Models;

namespace Search.API.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        // GET api/users
        [HttpGet]
        public ActionResult<IEnumerable<User>> Get()
        {
            return new[] { new User
                {
                    Nickname = "Deagle",
                    FirstName = "Desert Eagle",
                    LastName = "Pistol",
                    Email = "deagle@gmail.com"
                } };
        }

        // GET api/users/deagle@gmail.com
        [HttpGet("{email}")]
        public ActionResult<User> Get(string email)
        {
            return new User {
                Nickname = "Deagle",
                FirstName = "Desert Eagle",
                LastName = "Pistol",
                Email = "deagle@gmail.com"
            };
        }

        // POST api/users
        [HttpPost]
        public void Post([FromBody] User user)
        {
        }

        // PUT api/users/deagle@gmail.com
        [HttpPut("{email}")]
        public void Put(string email, [FromBody] User user)
        {
        }

        // DELETE api/users/deagle@gmail.com
        [HttpDelete("{email}")]
        public void Delete(string email)
        {
        }
    }
}