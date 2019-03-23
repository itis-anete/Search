using Microsoft.AspNetCore.Mvc;
using Search.SearchService;
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
        public ActionResult<IEnumerable<string>> Search([FromQuery] SearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(_searcher.Search(request));
        }

        private readonly Searcher _searcher;
    }
}