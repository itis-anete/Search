using Microsoft.AspNetCore.Mvc;
using Search.SearchService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        public SearchController(ServiceContainer services)
        {
            _searcher = services.Searcher;
        }
        
        [HttpGet]
        public IActionResult Search([FromQuery] SearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _searcher.Search(request);
            return View(result);
        }

        private readonly Searcher _searcher;
    }
}