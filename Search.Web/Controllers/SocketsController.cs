using BestSockets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Search.API.Controllers
{
    [Route("api/[controller]")]
    public class SocketsController : Controller
    {
        // GET api/sockets/message
        [HttpGet("{message}")]
        public IActionResult Get(string message)
        {
            using (var server = BestSocketServer<string, string>.StartListening(
                "127.0.0.1", 33060, request => new string(request.Reverse().ToArray())))
            {
                var response = BestSocketClient<string, string>.Send("127.0.0.1", 33060, message);
                return Ok(response);
            }
        }
    }
}
