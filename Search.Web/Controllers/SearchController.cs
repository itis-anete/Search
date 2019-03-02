using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Search.App;

namespace Search.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        // GET api/search
        [HttpGet]
        public ActionResult<IEnumerable<int>> Search(
            [Bind(Prefix = "url")] string url,
            [Bind(Prefix = "pattern")] string pattern)
        {
            return Ok(_server.Search(url, pattern));
        }

        private readonly Server _server = Server.Instance;
    }
}