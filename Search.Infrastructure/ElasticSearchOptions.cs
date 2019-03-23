using System;

namespace Search.Infrastructure
{
    public class ElasticSearchOptions
    {
        public Uri Url { get; set; }
        public string DocumentIndexName { get; set; } = "search-dot-net_main_index";
    }
}
