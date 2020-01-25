using Microsoft.Extensions.Hosting;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexHelpers;
using Search.IndexService.Internal;
using Search.IndexService.Models;
using Search.IndexService.SiteMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RailwayResults;

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

                IndexRequest processedRequest;
                try
                {
                    processedRequest = await Index(inProgressRequest);
                }
                catch (Exception error)
                {
                    var errorRequest = inProgressRequest.SetError(error.ToString());
                    indexRequestsQueue.Update(errorRequest);
                    continue;
                }
                indexRequestsQueue.Update(processedRequest);
            }
        }

        private async Task<IndexRequest> Index(InProgressIndexRequest request)
        {
            var siteHost = GetHost(request.Url);
            var siteHostWithDotBefore = '.' + siteHost;

            var siteMapUrl = new Uri(request.Url, "/sitemap.xml");
            var siteMap = await siteMapGetter.GetSiteMap(siteMapUrl);

            var urlsToIndex = new ConcurrentDictionary<Uri, byte>();
            urlsToIndex.TryAdd(request.Url, default);
            siteMap.Links
                .Where(uri =>
                    (uri.Host == siteHost || uri.Host.EndsWith(siteHostWithDotBefore)) &&
                    uri != request.Url)
                .Distinct()
                .ForEach(uri => urlsToIndex.TryAdd(uri, default));

            Result<string> indexingResult;
            var indexedUrls = new ConcurrentDictionary<Uri, byte>();
            var isUrlFromRequestIndexed = false;
            
            var semaphore = new SemaphoreSlim(32);
            var indexingTasks = new ConcurrentDictionary<Task, byte>();
            var completedIndexingTasks = new ConcurrentStack<Task<Result<string>>>();
            while (!urlsToIndex.IsEmpty)
            {
                Uri currentUrl;
                if (!isUrlFromRequestIndexed)
                {
                    currentUrl = request.Url;
                    isUrlFromRequestIndexed = true;
                }
                else
                    currentUrl = urlsToIndex.Keys.First();

                urlsToIndex.TryRemove(currentUrl, out _);
                indexedUrls.TryAdd(currentUrl, default);

                semaphore.Wait();
                indexingTasks.TryAdd(
                    IndexPage(currentUrl, request, siteHost, siteHostWithDotBefore, urlsToIndex, indexedUrls)
                        .ContinueWith(task =>
                        {
                            semaphore.Release();
                            indexingTasks.TryRemove(task, out _);
                            completedIndexingTasks.Push(task);
                        }),
                    default
                );
                
                indexingResult = CheckResultOfCompletedTasks();
                if (indexingResult.IsFailure)
                    return request.SetError(indexingResult.Error);

                while (urlsToIndex.IsEmpty)
                {
                    var indexingTask = indexingTasks.Keys.FirstOrDefault();
                    if (indexingTask == null)
                        break;
                    
                    indexingTasks.TryRemove(indexingTask, out _);
                    indexingTask.Wait();
                }

                if (!pagesPerSiteLimiter.IsLimitReached(indexedUrls.Count))
                    continue;
                Task.WaitAll(indexingTasks.Keys.ToArray());
                indexedUrls.Keys
                    .Where(uri => uri != request.Url)
                    .ForEach(x => _client.Delete(x.ToString(), _options.DocumentsIndexName));
                return request.SetError(
                    $"Не удалось проиндексировать сайт {request.Url} из-за ограничения в {pagesPerSiteLimiter.PagesPerSiteLimit} страниц на сайт " +
                    $"(найдено не менее {indexedUrls.Count} страниц). Проиндексирована только главная страница."
                );
            }

            Task.WaitAll(indexingTasks.Keys.ToArray());
            indexingResult = CheckResultOfCompletedTasks();
            if (indexingResult.IsFailure)
                return request.SetError(indexingResult.Error);

            return request.SetIndexed();

            Result<string> CheckResultOfCompletedTasks()
            {
                while (completedIndexingTasks.TryPop(out var completedTask))
                {
                    var result = completedTask.Result;
                    if (result.IsFailure)
                        return result;
                }

                return Result<string>.Success();
            }
        }

        private async Task<Result<string>> IndexPage(
            Uri currentUrl,
            IndexRequest request,
            string siteHost,
            string siteHostWithDotBefore,
            ConcurrentDictionary<Uri, byte> urlsToIndex,
            ConcurrentDictionary<Uri, byte> indexedUrls)
        {
            var html = await GetHtml(currentUrl);
            if (html == null)
            {
                if (currentUrl == request.Url)
                    return Result<string>.Fail($"Не удалось загрузить страницу {request.Url}");
                return Result<string>.Success(); // TODO: лучше не замалчивать ошибку
            }

            var parsedHtmlResult = Parser.HtmlToText.ParseHtml(html, request.Url);
            if (parsedHtmlResult.IsFailure)
            {
                if (currentUrl == request.Url)
                    return Result<string>.Fail($"Не удалось проиндексировать страницу {request.Url}");
                return Result<string>.Success(); // TODO: лучше не замалчивать ошибку
            }

            var parsedHtml = parsedHtmlResult.Value;
            parsedHtml.Links
                .Where(uri =>
                    (uri.Host == siteHost || uri.Host.EndsWith(siteHostWithDotBefore)) &&
                    !indexedUrls.ContainsKey(uri))
                .ForEach(uri => urlsToIndex.TryAdd(uri, default));

            var document = new Document
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
            return Result<string>.Success();
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
