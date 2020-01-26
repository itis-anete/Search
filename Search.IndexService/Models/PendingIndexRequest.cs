using System;

namespace Search.IndexService.Models
{
    public class PendingIndexRequest : IndexRequest
    {
        public override IndexRequestStatus Status => IndexRequestStatus.Pending;
        
        public PendingIndexRequest(Uri url, DateTime createdTime)
            : base(url, createdTime)
        {
        }

        public InProgressIndexRequest SetInProgress(DateTime startIndexingTime)
        {
            return new InProgressIndexRequest(Url, CreatedTime, startIndexingTime);
        }
    }
}
