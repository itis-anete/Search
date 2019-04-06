using System;

namespace Search.SearchService.Internal
{
    public class CacheRecord
    {
        public SearchResponse Response { get; }
        public TimeSpan CreationTime { get; set; }

        public CacheRecord(SearchResponse response, TimeSpan creationTime)
        {
            Response = response;
            CreationTime = creationTime;
        }
    }
}
