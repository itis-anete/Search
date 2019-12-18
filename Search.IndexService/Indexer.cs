using Microsoft.Extensions.Hosting;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService.Internal;
using Search.IndexService.Models;
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

        private const int pagesPerSiteLimit = 100;

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
                var inProgressRequest = request.SetInProgress();
                indexRequestsQueue.Update(inProgressRequest);

                var processedRequest = Index(inProgressRequest);
                indexRequestsQueue.Update(processedRequest);
            }

            return Task.CompletedTask;
        }

        private IndexRequest Index(InProgressIndexRequest request)
        {
            var urlsToParse = new Stack<Uri>();
            urlsToParse.Push(request.Url);

            var siteMap = SiteMapGetter.GetSiteMapContent(request.Url.ToString());
            siteMap.Links.ForEach(x => urlsToParse.Push(x));

            var siteHost = request.Url.Host;
            var indexedUrls = new HashSet<Uri>();
            while (urlsToParse.Any())
            {
                var currentUrl = urlsToParse.Pop();
                var html = GetHtml(currentUrl);
                indexedUrls.Add(currentUrl);
                if (html == null)
                    continue;

                var parsedHtml = Parser.HtmlToText.ParseHtml(html);

                parsedHtml.Links
                    .Where(x => x.Host.EndsWith(siteHost))
                    .Except(indexedUrls)
                    .ForEach(x => urlsToParse.Push(x));

                var document = new Document()
                {
                    Url = currentUrl,
                    IndexedTime = DateTime.UtcNow,
                    Title = parsedHtml.Title,
                    Text = parsedHtml.Text
                };

                _client.Index(document, desc => desc
                    .Id(document.Url.ToString())
                    .Index(_options.DocumentsIndexName)
                );

                if (indexedUrls.Count + urlsToParse.Count > pagesPerSiteLimit)
                {
                    indexedUrls
                        .Except(new[] { request.Url })
                        .ForEach(x => _client.Delete(x.ToString(), _options.DocumentsIndexName));
                    return request.SetError(
                        $"Can't index site {request.Url} due to limit of {pagesPerSiteLimit} pages per site. " +
                            "The main page was indexed only."
                    );
                }
            }

            return request.SetIndexed();
        }

        private string GetHtml(Uri url)
        {
            try
            {
                string html;
                using (var client = new HttpClient())
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                using (HttpContent content = response.Content)
                    html = content.ReadAsStringAsync().Result;

                return html;
            }
            catch 
            {
                return null;
            }
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
