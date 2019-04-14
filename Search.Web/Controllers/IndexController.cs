using Microsoft.AspNetCore.Mvc;
using Search.IndexService;
using Search.Indexer;

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
            HtmlParse(GetHtml(request));
            IndexRequest(url, GetHtml(request)); 
            return Ok();
        }

        private readonly Indexer _indexer;
    }
}