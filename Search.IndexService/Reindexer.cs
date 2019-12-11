using Microsoft.Extensions.Hosting;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Search.IndexService
{
    public class Reindexer : IHostedService
    {
        private readonly int reindexTime = -7;
        private readonly TimeSpan indexingFrequency = TimeSpan.FromMinutes(15);
        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly QueueForIndex _queueForIndex;

        private Timer _indexingTimer;

        public Reindexer(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            QueueForIndex queueForIndex)
        {
            _client = client;
            _options = options;
            _queueForIndex = queueForIndex;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _indexingTimer = new Timer(
                SearchOldPages,
                null,
                TimeSpan.Zero,
                indexingFrequency);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _indexingTimer?.Dispose();
            return Task.CompletedTask;
        }

        private void SearchOldPages(object state = null)
        {
            var responseFromElastic = _client.Search(search => search
               .Index(_options.DocumentsIndexName)
               .Query(desc => desc
                   .Match(match => match
                       .Field(x => x.IndexedTime < DateTime.Now.AddDays(reindexTime)))));

            foreach (var oldPage in responseFromElastic.Documents)
                _queueForIndex.AddToQueueElement(oldPage.Url);

        }
    }
}
