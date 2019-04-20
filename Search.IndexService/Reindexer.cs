using Search.Infrastructure;
using System;
using System.Threading;

namespace Search.IndexService
{
    public class Reindexer
    {
        public Reindexer(
            ElasticSearchClient client,
            Indexer indexer,
            bool autoReindexing = false,
            TimeSpan? indexingFrequency = null,
            TimeSpan? firstIndexingDeferral = null)
        {
            _client = client;
            _indexer = indexer;
            
            if (autoReindexing)
                _indexingTimer = new Timer(
                    ReindexAll,
                    null,
                    firstIndexingDeferral ?? default(TimeSpan),
                    indexingFrequency ?? TimeSpan.FromMinutes(15));
        }

        public void ReindexAll() => ReindexAll(null);

        private readonly ElasticSearchClient _client;
        private readonly Indexer _indexer;
        private readonly Timer _indexingTimer;

        private void ReindexAll(object state)
        {
            throw new NotImplementedException();

            var response = _client.Search(desc => desc
                .Query(query => query.MatchAll())
            );

            foreach (var document in response.Documents)
            {
                var indexRequest = new IndexRequest
                {
                    //Url = document.Url
                };
                _indexer.Index(indexRequest);
            }
        }
    }
}
