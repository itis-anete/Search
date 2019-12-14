using Microsoft.AspNetCore.Mvc;
using Search.Core.Extensions;
using Search.IndexService;
using Search.IndexService.Dto;
using System;
using System.Linq;

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
            if (!url.IsAbsoluteUri)
                return BadRequest($"Url {url} is not absolute");

            var result = _queueForIndex.AddToQueueElement(url);
            return result.IsSuccess
                ? Ok()
                : StatusCode(result.Error.ToInt());
        }

        [HttpGet]
        public IActionResult GetQueue()
        {
            var requests = _queueForIndex.GetAllElementsQueue();
            if (requests.IsFailure)
                return StatusCode(requests.Error.ToInt());

            return Ok(
                requests.Value.Select(x => IndexRequestDto.FromModel(x))
            );
        }

        private readonly QueueForIndex _queueForIndex;
    }
}