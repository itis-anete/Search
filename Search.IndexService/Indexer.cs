using Microsoft.Extensions.Hosting;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexHelpers;
using Search.IndexService.Internal;
using Search.IndexService.Models;
using Search.IndexService.SiteMap;
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
        private readonly SiteMapGetter siteMapGetter;
        private readonly PagesPerSiteLimiter pagesPerSiteLimiter;
        private readonly HttpClient httpClient;

        public Indexer(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            QueueForIndex indexRequestsQueue,
            IHttpClientFactory httpClientFactory,
            SiteMapGetter siteMapGetter,
            PagesPerSiteLimiter pagesPerSiteLimiter)
        {
            _client = client;
            _options = options;
            this.indexRequestsQueue = indexRequestsQueue;
            this.siteMapGetter = siteMapGetter;
            this.pagesPerSiteLimiter = pagesPerSiteLimiter;
            httpClient = httpClientFactory.CreateClient("Page downloader");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureIndexInElasticCreated();

            while (!stoppingToken.IsCancellationRequested)
            {
                var request = indexRequestsQueue.WaitForIndexElement();
                var inProgressRequest = request.SetInProgress();
                indexRequestsQueue.Update(inProgressRequest);

                var processedRequest = await Index(inProgressRequest);
                indexRequestsQueue.Update(processedRequest);
            }
        }

        private async Task<IndexRequest> Index(InProgressIndexRequest request)
        {
            var siteHost = GetHost(request.Url);

            var siteMapUrl = new Uri(request.Url, "/sitemap.xml");
            var siteMap = await siteMapGetter.GetSiteMap(siteMapUrl);

            var urlsToParse = new Stack<Uri>();
            siteMap.Links
                .Where(x => x.Host.EndsWith(siteHost))
                .Except(new[] { request.Url })
                .Distinct()
                .ForEach(x => urlsToParse.Push(x));
            urlsToParse.Push(request.Url);

            var indexedUrls = new HashSet<Uri>();
            while (urlsToParse.Any())
            {
                var currentUrl = urlsToParse.Pop();
                var html = await GetHtml(currentUrl);
                indexedUrls.Add(currentUrl);
                if (html == null)
                {
                    if (currentUrl == request.Url)
                        return request.SetError($"Не удалось загрузить страницу {request.Url}");
                    continue;
                }

                var parsedHtml = Parser.HtmlToText.ParseHtml(html, request.Url);
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

                var foundUrlsCount = indexedUrls.Count + urlsToParse.Count;
                if (pagesPerSiteLimiter.IsLimitReached(foundUrlsCount))
                {
                    indexedUrls
                        .Except(new[] { request.Url })
                        .ForEach(x => _client.Delete(x.ToString(), _options.DocumentsIndexName));
                    return request.SetError(
                        $"Не удалось проиндексировать сайт {request.Url} из-за ограничения в {pagesPerSiteLimiter.PagesPerSiteLimit} страниц на сайт " +
                        $"(найдено не менее {foundUrlsCount} страниц). Проиндексирована только главная страница."
                    );
                }
            }

            return request.SetIndexed();
        }

        private async Task<string> GetHtml(Uri url)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStringAsync();
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

        private static string GetHost(Uri url)
        {
            var hostStr = url.Host.ToString();

            var lastDotIndex = hostStr.LastIndexOf('.');
            if (lastDotIndex < 0)
                return hostStr;

            var nextToLastDotIndex = hostStr.Substring(0, lastDotIndex).LastIndexOf('.');
            if (nextToLastDotIndex < 0)
                return hostStr;

            return hostStr.Substring(nextToLastDotIndex + 1, hostStr.Length - nextToLastDotIndex - 1);
        }
    }
}
