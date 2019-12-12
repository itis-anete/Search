using Microsoft.AspNetCore.Mvc;
using Search.Core.Extensions;
using Search.IndexService;
using System;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _queueForIndex.AddToQueueElement(url);
            return result.IsSuccess
                ? Ok()
                : StatusCode(result.Error.ToInt());
        }

        [HttpGet]
        public IActionResult GetQueue()
        {
            var result = _queueForIndex.GetAllElementsQueue();
            return result.IsSuccess
                ? (IActionResult)Ok(result.Value)
                : StatusCode(result.Error.ToInt());
        }

        private readonly QueueForIndex _queueForIndex;
    }
}