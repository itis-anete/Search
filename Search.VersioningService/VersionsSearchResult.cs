using System;

namespace Search.VersioningService
{
    public class VersionsSearchResult
    {
        public Uri Url { get; set; }
        public string Title { get; set; }
        public DateTime IndexedTime { get; set; }
    }
}
