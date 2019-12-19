using Microsoft.Extensions.Hosting;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService.Dto;
using Search.IndexService.Models;
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
        private readonly ElasticSearchClient<Document> _documentsClient;
        private readonly ElasticSearchClient<IndexRequestDto> _requestsClient;
        private readonly ElasticSearchOptions _options;
        private readonly QueueForIndex _queueForIndex;

        private Timer _indexingTimer;

        public Reindexer(
            ElasticSearchClient<Document> documentsClient,
            ElasticSearchClient<IndexRequestDto> requestsClient,
            ElasticSearchOptions options,
            QueueForIndex queueForIndex)
        {
            _documentsClient = documentsClient;
            _requestsClient = requestsClient;
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
            var responseWithDocuments = _documentsClient.Search(search => search
                .Index(_options.DocumentsIndexName)
                .Query(desc =>
                    desc.DateRange(d => d
                        .Field(x => x.IndexedTime)
                        .LessThan(DateTime.UtcNow.AddDays(reindexTime))
                    )
                )
            );
            if (!responseWithDocuments.IsValid)
                return;

            var documentsUrls = responseWithDocuments.Documents
                .Select(x => x.Url)
                .ToArray();
            var responseWithRequests = _requestsClient.Search(search => search
                .Index(_options.RequestsIndexName)
                .Query(desc =>
                    desc.Terms(t => t
                        .Field(x => x.Url)
                        .Terms(documentsUrls)
                    )
                    &&
                    desc.Term(t => t
                        .Field(x => x.Status)
                        .Value(IndexRequestStatus.Indexed)
                    )
                )
            );
            if (!responseWithRequests.IsValid)
                return;

            var urlsToReindex = responseWithRequests.Documents
                .Select(x => x.Url)
                .ToArray();
            foreach (var url in urlsToReindex)
                _queueForIndex.AddToQueueElement(url);
        }
    }
}
