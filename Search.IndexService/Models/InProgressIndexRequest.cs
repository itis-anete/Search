using System;

namespace Search.IndexService.Models
{
    public class InProgressIndexRequest : IndexRequest
    {
        public InProgressIndexRequest(Uri url, DateTime createdTime)
            : base(url, createdTime)
        {
        }

        public override IndexRequestStatus Status => IndexRequestStatus.InProgress;

        public IndexedIndexRequest SetIndexed()
        {
            return new IndexedIndexRequest(Url, CreatedTime);
        }

        public ErrorIndexRequest SetError(string errorMessage)
        {
            return new ErrorIndexRequest(Url, CreatedTime, errorMessage);
        }
    }
}
