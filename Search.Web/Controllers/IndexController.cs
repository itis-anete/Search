using Microsoft.AspNetCore.Mvc;
using Search.IndexService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        public IndexController(Indexer indexer)
        {
            _indexer = indexer;
        }

        // POST api/index
        [HttpPost]
        public IActionResult Index([FromQuery] IndexRequest request)
        {
            _indexer.Index(request);
            return Ok();
        }

        private readonly Indexer _indexer;
    }
}