using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Search.IndexService
{
    public class Reindexer:IDisposable
    {
        public Reindexer(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            Indexer indexer,
            QueueForIndex queueForIndex)
        {
            _client = client;
            _options = options;
            _indexer = indexer;
            ReindexTime = -7;
            _queueForIndex = queueForIndex;
            EnsureIndicesCreated();
            reindexTask = Task.Run(ReindexTask);
        }

       
        private int ReindexTime { get; set; }
        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly Indexer _indexer;
        private readonly Timer _indexingTimer;
        private QueueForIndex _queueForIndex;
        private readonly Task reindexTask;

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

        private void EnsureIndicesCreated()
        {
            var response = _client.IndexExists(_options.DocumentsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.RequestsIndexName, index => index
                .Settings(ElasticSearchOptions.AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<Document>(map => map
                        .Properties(properties => properties
                            .Text(ElasticSearchOptions.TitleProperty)
                            .Text(ElasticSearchOptions.TextProperty)
                            .Keyword(ElasticSearchOptions.UrlProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );

            response = _client.IndexExists(_options.RequestsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.RequestsIndexName, index => index
                .Settings(ElasticSearchOptions.AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<IndexRequest>(map => map.AutoMap())
                )
            );
        }
        private async Task ReindexTask()
        {
            while (true)
            {
                SearchOldPages();
            }
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
