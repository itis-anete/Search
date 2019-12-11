using Microsoft.AspNetCore.Mvc;
using Search.Core.Entities;
using Search.IndexService;
using System;
using System.Collections.Generic;

namespace Search.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class IndexController : ControllerBase
    {
        public IndexController(ServiceContainer services)
        {
            _queueForIndex = services.QueueForIndex;
        }
        
        [HttpPost]
        public IActionResult Index([FromBody] Uri url)
        {
            _queueForIndex.AddToQueueElement(url);
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