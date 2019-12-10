using Search.IndexService;
using Search.SearchService;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Searcher searcher,
            //Indexer indexer,
            Reindexer reindexer,
            QueueForIndex queueForIndex)
        {
            Searcher = searcher;
            //Indexer = indexer;
            Reindexer = reindexer;
            QueueForIndex = queueForIndex;
        }

        public Searcher Searcher { get; }
        //public Indexer Indexer { get; }
        public Reindexer Reindexer { get; }

        public QueueForIndex QueueForIndex { get; }
    }
}
