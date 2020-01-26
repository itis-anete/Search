using Microsoft.Extensions.Hosting;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService.Dbo;
using Search.IndexService.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Search.IndexService
{
    public class Reindexer : IHostedService
    {
        private readonly TimeSpan pageTimeToLive = TimeSpan.FromDays(1);
        private readonly TimeSpan indexingFrequency = TimeSpan.FromDays(1);
        private readonly ElasticSearchClient<IndexRequestDbo> _requestsClient;
        private readonly ElasticSearchOptions _options;
        private readonly QueueForIndex _queueForIndex;

        private Timer _indexingTimer;

        public Reindexer(
            ElasticSearchClient<IndexRequestDbo> requestsClient,
            ElasticSearchOptions options,
            QueueForIndex queueForIndex)
        {
            _requestsClient = requestsClient;
            _options = options;
            _queueForIndex = queueForIndex;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _indexingTimer = new Timer(
                SearchOldPages,
                null,
                indexingFrequency,
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
            var response = _requestsClient.Search(search => search
                .Index(_options.RequestsIndexName)
                .Query(desc =>
                    desc.Term(t => t
                        .Field(request => request.Status)
                        .Value(IndexRequestStatus.Indexed)
                    )
                    &&
                    desc.DateRange(d => d
                        .Field(request => request.CreatedTime) // TODO: изменить на indexedTime
                        .LessThan(DateTime.UtcNow.Subtract(pageTimeToLive))
                    )
                )
            );
            if (!response.IsValid)
                return;

            var urlsToReindex = response.Documents
                .Select(x => x.Url)
                .ToArray();
            foreach (var url in urlsToReindex)
                _queueForIndex.AddToQueueElement(url);
        }
    }
}
