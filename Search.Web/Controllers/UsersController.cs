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
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET api/values
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

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<User> Get(string email)
        {
            return new User {
                Nickname = "Deagle",
                FirstName = "Desert Eagle",
                LastName = "Pistol",
                Email = "deagle@gmail.com"
            };
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] User user)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(string email, [FromBody] User user)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string email)
        {
        }
    }
}