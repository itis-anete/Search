using Search.IndexService;
using Search.FearchFervice;

namespace Search.Web
{
    public class ServiceContainer
    {
        public ServiceContainer(
            Fearcher fearcher,
            QueueForIndex queueForIndex)
        {
            Fearcher = fearcher;
            QueueForIndex = queueForIndex;
        }

        public Fearcher Fearcher { get; }

        public QueueForIndex QueueForIndex { get; }
    }
}
