using System;

namespace Search.IndexService.Models
{
    public class PendingIndexRequest : IndexRequest
    {
        public PendingIndexRequest(Uri url, DateTime createdTime)
            : base(url, createdTime)
        {
        }

        public override IndexRequestStatus Status => IndexRequestStatus.Pending;

        public InProgressIndexRequest SetInProgress()
        {
            return new InProgressIndexRequest(Url, CreatedTime);
        }
    }
}
