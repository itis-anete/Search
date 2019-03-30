using System;

namespace Search.Infrastructure
{
    public class ElasticSearchOptions
    {
        public Uri Url { get; set; }
        public string DocumentsIndexName { get; set; } = "documents_index";

        public bool EnableVersioning { get; set; }
        public string VersionsIndexName { get; set; } = "versions_index";
    }
}
