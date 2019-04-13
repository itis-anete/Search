using Microsoft.AspNetCore.Mvc;
using Search.VersioningService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class VersionsSearchController : ControllerBase
    {
        public VersionsSearchController(ServiceContainer services)
        {
            _searcher = services.VersionsSearcher;
        }
        
        [HttpGet]
        public IActionResult Search([FromQuery] VersionsSearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(_searcher.Search(request));
        }

        private readonly VersionsSearcher _searcher;
    }
}
