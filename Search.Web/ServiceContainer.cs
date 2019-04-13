using Search.IndexService;
using Search.SearchService;
using Search.VersioningService;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Searcher searcher,
            Indexer indexer,
            VersionsSearcher versionsSearcher)
        {
            Searcher = searcher;
            Indexer = indexer;
            VersionsSearcher = versionsSearcher;
        }

        public Searcher Searcher { get; }
        public Indexer Indexer { get; }
        public VersionsSearcher VersionsSearcher { get; }
    }
}
