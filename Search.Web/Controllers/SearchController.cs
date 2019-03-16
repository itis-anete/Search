using Microsoft.AspNetCore.Mvc;
using Search.Core.Entities;
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

        // POST api/search
        [HttpPost]
        public ActionResult Index(
            [FromQuery] string title,
            [FromBody] string body)
        {
            var info = new DocumentInfo
            {
                Title = title,
                Text = body
            };
            _searcher._searchDatabase.Add(info);
            return Ok();
        }

        // GET api/search
        [HttpGet]
        public ActionResult<IEnumerable<string>> Search([FromQuery] string query)
        {
            var searchQuery = new SearchRequest
            {
                Query = query
            };

            return Ok(_searcher.Search(searchQuery));
        }

        private readonly Searcher _searcher;
    }
}