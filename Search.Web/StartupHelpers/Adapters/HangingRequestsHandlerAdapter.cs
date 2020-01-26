using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Search.IndexService;

namespace Search.Web.StartupHelpers.Adapters
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
            _handler.CheckHangingRequests();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}