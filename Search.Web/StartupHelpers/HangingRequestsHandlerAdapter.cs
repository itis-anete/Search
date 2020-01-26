using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Search.IndexService;

namespace Search.Web.StartupHelpers
{
    public class HangingRequestsHandlerAdapter : IHostedService
    {
        private readonly HangingRequestsHandler _handler;

        public HangingRequestsHandlerAdapter(HangingRequestsHandler handler)
        {
            _handler = handler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var (success, message) = _handler.CheckHangingRequests();
            if (!success)
                throw new Exception(message);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}