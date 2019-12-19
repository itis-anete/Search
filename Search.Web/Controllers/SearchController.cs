using Microsoft.AspNetCore.Mvc;
using Search.Core.Extensions;
using Search.FearchFervice;

namespace Search.Web.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        public SearchController(Fearcher fearcher)
        {
            _fearcher = fearcher;
        }
        
        [HttpGet]
        public IActionResult Search([FromQuery] FearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (request == null)
                return BadRequest("Не заданы параметры поискового запроса.");

            var result = _fearcher.Search(request);
            return result.IsSuccess
                ? (IActionResult)Ok(result.Value)
                : StatusCode(result.Error.ToInt());
        }

        private readonly Fearcher _fearcher;
    }
}