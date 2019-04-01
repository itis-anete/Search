using Search.SearchService;
using System;

namespace Search.VersioningService
{
    public class VersionsSearchRequest : SearchRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
