using Search.IndexService;
using Search.SearchService;
using Search.VersioningService;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Searcher searcher,
            VersionsSearcher versionsSearcher,
            Indexer indexer,
            Reindexer reindexer)
        {
            Searcher = searcher;
            VersionsSearcher = versionsSearcher;
            Indexer = indexer;
            Reindexer = reindexer;
        }

        public Searcher Searcher { get; }
        public VersionsSearcher VersionsSearcher { get; }
        public Indexer Indexer { get; }
        public Reindexer Reindexer { get; }
    }
}
