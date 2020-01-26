using System;

namespace Search.IndexService.Models
{
    public class IndexedIndexRequest : IndexRequest
    {
        public DateTime StartIndexingTime { get; }
        
        public int IndexedPagesCount { get; }
        
        public DateTime EndIndexingTime { get; }

        public override IndexRequestStatus Status => IndexRequestStatus.Indexed;
        
        public IndexedIndexRequest(Uri url,
            DateTime createdTime,
            DateTime startIndexingTime,
            int indexedPagesCount,
            DateTime endIndexingTime)
            : base(url, createdTime)
        {
            StartIndexingTime = startIndexingTime;
            IndexedPagesCount = indexedPagesCount;
            EndIndexingTime = endIndexingTime;
        }
    }
}
