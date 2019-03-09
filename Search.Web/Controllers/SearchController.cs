using Microsoft.AspNetCore.Mvc;
using Search.Infrastructure;
using System.Collections.Generic;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(Searcher searcher)
        {
            _searcher = searcher;
        }

        // GET api/search
        [HttpGet]
        public ActionResult<IEnumerable<string>> Search([FromQuery] string query)
        {
            return Ok(_searcher.Search(query));
        }

        private readonly Searcher _searcher;
    }
}