using Microsoft.AspNetCore.Mvc;
using Search.Core.Entities;
using Search.IndexService;
using System.Collections.Generic;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class QueueController : Controller
    {
        private readonly QueueForIndex _queueForIndex;
        public QueueController(ServiceContainer services)
        {
            _queueForIndex = services.QueueForIndex;
        }

        [HttpGet]
        public IEnumerable<IndexRequest> GetQueue()
        {
            return _queueForIndex.GetAllElementsQueue(); 
        }
    }
}
