using System;

namespace Search.IndexService.Models
{
    public class IndexedIndexRequest : IndexRequest
    {
        public IndexedIndexRequest(Uri url, DateTime createdTime)
            : base(url, createdTime)
        {
        }

        public override IndexRequestStatus Status => IndexRequestStatus.Indexed;
    }
}
