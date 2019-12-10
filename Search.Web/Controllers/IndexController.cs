using Microsoft.AspNetCore.Mvc;
using Search.Core.Entities;
using Search.IndexService;
using System.Collections.Generic;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class IndexController : ControllerBase
    {
        public IndexController(ServiceContainer services)
        {
            _queueForIndex = services.QueueForIndex;
        }
        
        [HttpPost]
        public IActionResult Index([FromQuery] IndexRequest request)
        {
            _queueForIndex.AddToQueueElement(request);
            return Ok();
        }

        [HttpGet]
        public IEnumerable<IndexRequest> GetQueue()
        {
            return _queueForIndex.GetAllElementsQueue();
        }

        private readonly QueueForIndex _queueForIndex;
    }
}