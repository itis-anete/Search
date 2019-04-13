using Search.SearchService;
using System;

namespace Search.VersioningService
{
    public class VersionsSearchResult : SearchResult
    {
        public DateTime IndexedTime { get; set; }
    }
}
