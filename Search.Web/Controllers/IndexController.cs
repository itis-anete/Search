using Microsoft.AspNetCore.Mvc;
using Search.Core.Entities;
using Search.IndexService;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class IndexController : ControllerBase
    {
        public IndexController(ServiceContainer services)
        {
            _indexer = services.Indexer;
        }
        
        [HttpPost]
        public IActionResult Index([FromQuery] IndexRequest request)
        {
            _indexer.Index(request);
            return Ok();
        }

        private readonly Indexer _indexer;
    }
}