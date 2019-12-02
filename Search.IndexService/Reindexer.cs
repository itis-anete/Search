using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Linq;
using System.Threading;

namespace Search.IndexService
{
    public class Reindexer
    {
        public Reindexer(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            Indexer indexer,
            //bool autoReindexing = false,
            //TimeSpan? indexingFrequency = null,
            QueueForIndex queueForIndex)
        // TimeSpan? firstIndexingDeferral = null)
        {
            _client = client;
            _options = options;
            _indexer = indexer;
            ReindexTime = -7;
            _queueForIndex = queueForIndex;
            //if (autoReindexing)
            //    _indexingTimer = new Timer(
            //        ReindexAll,
            //        null,
            //        firstIndexingDeferral ?? default(TimeSpan),
            //        indexingFrequency ?? TimeSpan.FromMinutes(15));
        }

        //public void ReindexAll() => ReindexAll(null);
        private int ReindexTime { get; set; }
        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly Indexer _indexer;
        private readonly Timer _indexingTimer;
        private QueueForIndex _queueForIndex;

        //private void ReindexAll(object state)
        //{
        //    var response = _client.Search(desc => desc
        //        .Index(_options.DocumentsIndexName)
        //        .Query(query => query.MatchAll())
        //    );

        //    foreach (var document in response.Documents)
        //    {
        //        var indexRequest = new IndexRequest { Url = document.Url };
        //        _indexer.Index(indexRequest);
        //    }
        //}

        public void SearchOldPages()
        {
            var responseFromElastic = _client.Search(search => search
               .Index(_options.DocumentsIndexName)
               .Query(desc => desc
                   .Match(match => match
                       .Field(x => x.IndexedTime < DateTime.Now.AddDays(ReindexTime)))));
            var oldPages = responseFromElastic.Documents
                .Select(doc => new IndexRequest
                {
                    Url = doc.Url,
                    CreatedTime = DateTime.Now,
                    Status = IndexRequestStatus.Pending
                }).ToList();

            foreach (var item in oldPages)
            {
                _queueForIndex.AddToQueueElement(item);
            }

        }
    }
}
