using Search.IndexService;
using Search.SearchService;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Searcher searcher,
            Indexer indexer,
            Reindexer reindexer)
        {
            Searcher = searcher;
            Indexer = indexer;
            Reindexer = reindexer;
        }

        public Searcher Searcher { get; }
        public Indexer Indexer { get; }
        public Reindexer Reindexer { get; }
    }
}
