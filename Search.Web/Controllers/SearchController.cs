using Microsoft.AspNetCore.Mvc;
using Search.Core.Extensions;
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
            if (request == null)
                return BadRequest("Не заданы параметры поискового запроса.")

            var result = _searcher.Search(request);
            return result.IsSuccess
                ? (IActionResult)Ok(result.Value)
                : StatusCode(result.Error.ToInt());
        }

        private readonly Searcher _searcher;
    }
}