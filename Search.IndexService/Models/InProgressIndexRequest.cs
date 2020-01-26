using System;

namespace Search.IndexService.Models
{
    public class InProgressIndexRequest : IndexRequest
    {
        public DateTime StartIndexingTime { get; }
        
        public int IndexedPagesCount { get; private set; }
        
        public int FoundPagesCount { get; private set; }
        
        public override IndexRequestStatus Status => IndexRequestStatus.InProgress;
        
        public InProgressIndexRequest(
            Uri url,
            DateTime createdTime,
            DateTime startIndexingTime,
            int indexedPagesCount = 0,
            int foundPagesCount = 0)
            : base(url, createdTime)
        {
            StartIndexingTime = startIndexingTime;
            UpdatePagesCounts(indexedPagesCount, foundPagesCount);
        }

        public IndexedIndexRequest SetIndexed(DateTime endIndexingTime)
        {
            return new IndexedIndexRequest(Url, CreatedTime, StartIndexingTime, IndexedPagesCount, endIndexingTime);
        }

        public ErrorIndexRequest SetError(string errorMessage)
        {
            return new ErrorIndexRequest(Url, CreatedTime, errorMessage);
        }

        public void UpdatePagesCounts(int indexedPagesCount, int foundPagesCount)
        {
            if (indexedPagesCount > foundPagesCount)
                throw new ArgumentException(
                    "Количество проиндексированных страниц не может быть больше количества найденных");

            IndexedPagesCount = indexedPagesCount;
            FoundPagesCount = foundPagesCount;
        }
    }
}
