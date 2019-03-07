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
            using (var server = SocketServer<string>.StartNew(
                ip: "127.0.0.1",
                port: 33060,
                onRequest: request => new string(request.Reverse().ToArray())))
            {
                var response = SocketClient<string>.Send("127.0.0.1", 33060, message);
                return Ok(response);
            }
        }
    }
}
