using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Threading;

namespace Search.IndexService
{
    public class Reindexer
    {
        public Reindexer(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            Indexer indexer,
            bool autoReindexing = false,
            TimeSpan? indexingFrequency = null,
            TimeSpan? firstIndexingDeferral = null)
        {
            _client = client;
            _options = options;
            _indexer = indexer;
            
            if (autoReindexing)
                _indexingTimer = new Timer(
                    ReindexAll,
                    null,
                    firstIndexingDeferral ?? default(TimeSpan),
                    indexingFrequency ?? TimeSpan.FromMinutes(15));
        }

        public void ReindexAll() => ReindexAll(null);

        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly Indexer _indexer;
        private readonly Timer _indexingTimer;

        private void ReindexAll(object state)
        {
            var response = _client.Search(desc => desc
                .Index(_options.DocumentsIndexName)
                .Query(query => query.MatchAll())
            );

            foreach (var document in response.Documents)
            {
                var indexRequest = new IndexRequest { Url = document.Url };
                _indexer.Index(indexRequest);
            }
        }
    }
}
