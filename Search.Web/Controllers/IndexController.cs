using Microsoft.AspNetCore.Mvc;
using Search.IndexService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        public IndexController(ServiceContainer services)
        {
            _indexer = services.Indexer;
        }

        // POST api/index
        [HttpPost]
        public IActionResult Index([FromQuery] IndexRequest request)
        {
            _indexer.Index(request);
            DocumentInfo docInfo = new DocumentInfo(request.url);
            return Ok();
        }

        private readonly Indexer _indexer;
    }
}