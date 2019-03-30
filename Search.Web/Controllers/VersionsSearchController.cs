using Microsoft.AspNetCore.Mvc;
using Search.VersioningService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionsSearchController : ControllerBase
    {
        public VersionsSearchController(VersionsSearcher searcher)
        {
            _searcher = searcher;
        }

        // GET api/versionssearch
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
