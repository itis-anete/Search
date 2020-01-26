using System.Threading;
using System.Threading.Tasks;
using Search.IndexService;

namespace Search.Web.StartupHelpers.Adapters
{
    public class IndexerAdapter : AsyncBackgroundService
    {
        private readonly Indexer _indexer;

        public IndexerAdapter(Indexer indexer)
        {
            _indexer = indexer;
        }
        
        protected override Task RunAsync(CancellationToken stoppingToken)
        {
            return _indexer.RunAsync(stoppingToken);
        }
    }
}