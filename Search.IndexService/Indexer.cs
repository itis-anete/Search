using Microsoft.Extensions.Hosting;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Search.IndexService
{
    public class Indexer : BackgroundService
    {
        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly QueueForIndex indexRequestsQueue;

        public Indexer(ElasticSearchClient<Document> client, ElasticSearchOptions options, QueueForIndex indexRequestsQueue)
        {
            _client = client;
            _options = options;
            this.indexRequestsQueue = indexRequestsQueue;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureIndexInElasticCreated();

            while (!stoppingToken.IsCancellationRequested)
            {
                var request = indexRequestsQueue.WaitForIndexElement();
                Index(request);
            }

            return Task.CompletedTask;
        }

        private void Index(IndexRequest request)
        {
            var urlsToParse = new Stack<Uri>();
            urlsToParse.Push(request.Url);

            var siteHost = request.Url.Host;
            while (urlsToParse.Any())
            {
                var currentUrl = urlsToParse.Pop();
                var html = GetHtml(currentUrl);
                var parsedHtml = Parser.HtmlToText.ParseHtml(html);

                parsedHtml.Links
                    .Where(x => x.Host.EndsWith(siteHost))
                    .ForEach(x => urlsToParse.Push(x));

                var document = new Document()
                {
                    Url = request.Url,
                    IndexedTime = DateTime.UtcNow,
                    Title = parsedHtml.Title,
                    Text = parsedHtml.Text
                };

                _client.Index(document, desc => desc
                    .Id(document.Url.ToString())
                    .Index(_options.DocumentsIndexName)
                );
            }

            indexRequestsQueue.UpdateStatus(request.Url, IndexRequestStatus.Indexed);
        }

        private string GetHtml(Uri url)
        {
            string html;
            using (var client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            using (HttpContent content = response.Content)
                html = content.ReadAsStringAsync().Result;

            return html;
        }

        private void EnsureIndexInElasticCreated()
        {
            var response = _client.IndexExists(_options.DocumentsIndexName);
            if (response.Exists)
                return;
            
            _client.CreateIndex(_options.DocumentsIndexName, index => index
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
        }
    }
}
