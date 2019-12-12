using Search.IndexService;
using Search.SearchService;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Searcher searcher,
            QueueForIndex queueForIndex)
        {
            Searcher = searcher;
            QueueForIndex = queueForIndex;
        }

        public Searcher Searcher { get; }

        public QueueForIndex QueueForIndex { get; }
    }
}
