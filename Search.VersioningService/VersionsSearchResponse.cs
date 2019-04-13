using System.Collections.Generic;

namespace Search.VersioningService
{
    public class VersionsSearchResponse
    {
        public IList<VersionsSearchResult> Results { get; set; }
    }
}
