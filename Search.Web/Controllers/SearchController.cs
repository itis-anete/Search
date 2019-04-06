using Microsoft.AspNetCore.Mvc;
using Search.SearchService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(ServiceContainer services)
        {
            _searcher = services.Searcher;
        }

        // GET api/search
        [HttpGet]
        public IActionResult Search([FromQuery] SearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(_searcher.Search(request));
        }

        private readonly Searcher _searcher;
    }
}