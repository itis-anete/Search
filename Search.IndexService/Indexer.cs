using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexHelpers;
using Search.IndexService.Internal;
using Search.IndexService.Models;
using Search.IndexService.SiteMap;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RailwayResults;
using Search.Core.Extensions;

namespace Search.IndexService
{
    public class Indexer
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

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            EnsureIndexInElasticCreated();

            while (!stoppingToken.IsCancellationRequested)
            {
                var request = indexRequestsQueue.WaitForIndexElement();
                var inProgressRequest = request.SetInProgress(DateTime.UtcNow);
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
            if (pagesPerSiteLimiter.IsLimitReached(siteMap.Links.Length))
                return request.SetError(GetTooManyPagesErrorMessage(
                    request.Url, siteMap.Links.Length, pagesPerSiteLimiter.PagesPerSiteLimit)
                );

            var urlsToIndex = new ConcurrentDictionary<Uri, byte>();
            urlsToIndex.TryAdd(request.Url, default);
            siteMap.Links
                .Where(uri =>
                    (uri.Host == siteHost || uri.Host.EndsWith(siteHostWithDotBefore)) &&
                    uri != request.Url)
                .Distinct()
                .ForEach(uri => urlsToIndex.TryAdd(uri, default));

            request.UpdatePagesCounts(0, urlsToIndex.Count);
            indexRequestsQueue.Update(request);

            Result<string> indexingResult;
            var indexedUrls = new ConcurrentDictionary<Uri, byte>();
            var isUrlFromRequestIndexed = false;
            var indexedUrlsRoughCount = 0;
            
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

                if (semaphore.CurrentCount == 0)
                {
                    var indexedPagesCount = indexedUrls.Count;
                    var foundPagesCount = indexedPagesCount + urlsToIndex.Count;
                    request.UpdatePagesCounts(indexedPagesCount, foundPagesCount);
#pragma warning disable 4014
                    indexRequestsQueue.UpdateAsync(request);
#pragma warning restore 4014
                }
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
                
                if (indexedUrls.Count / 200 > indexedUrlsRoughCount / 200)
                {
                    GC.Collect();
                    indexedUrlsRoughCount = indexedUrls.Count;
                }

                // ReSharper disable once InvertIf
                if (pagesPerSiteLimiter.IsLimitReached(indexedUrls.Count))
                {
                    Task.WaitAll(indexingTasks.Keys.ToArray());
                    _client.DeleteMany(
                        indexedUrls.Keys
                            .Where(uri => uri != request.Url)
                            .Select(uri => uri.ToString()),
                        _options.DocumentsIndexName
                    );
                    return request.SetError(GetTooManyPagesErrorMessage(
                        request.Url,
                        indexedUrls.Count,
                        pagesPerSiteLimiter.PagesPerSiteLimit)
                    );
                }
            }

            Task.WaitAll(indexingTasks.Keys.ToArray());
            indexingResult = CheckResultOfCompletedTasks();
            if (indexingResult.IsFailure)
                return request.SetError(indexingResult.Error);

            Debug.Assert(urlsToIndex.Count == 0,
                "После завершения индексации остались непроиндексированные страницы");
            request.UpdatePagesCounts(indexedUrls.Count, indexedUrls.Count);
            return request.SetIndexed(DateTime.UtcNow);

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
            var htmlResult = await GetHtml(currentUrl);
            if (htmlResult.IsFailure)
            {
                if (currentUrl == request.Url)
                    return Result<string>.Fail($"Не удалось загрузить страницу {request.Url}\n" +
                                               htmlResult.Error);
                return Result<string>.Success();
            }

            var html = htmlResult.Value;
            var parsedHtmlResult = Parser.HtmlToText.ParseHtml(html, request.Url);
            if (parsedHtmlResult.IsFailure)
            {
                if (currentUrl == request.Url)
                    return Result<string>.Fail($"Не удалось проиндексировать страницу {request.Url}\n" +
                                               parsedHtmlResult.Error);
                return Result<string>.Success();
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

#pragma warning disable 4014
            _client.IndexAsync(document, desc => desc
#pragma warning restore 4014
                .Id(document.Url.ToString())
                .Index(_options.DocumentsIndexName)
            );
            return Result<string>.Success();
        }

        private async Task<Result<string, string>> GetHtml(Uri url)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return Result<string, string>.Fail($"Страница {url} не найдена");
                    if (response.StatusCode.IsServerError())
                        return Result<string, string>.Fail($"Страница {url} недоступна");
                    return Result<string, string>.Fail(response.ReasonPhrase);
                }

                var content = await response.Content.ReadAsStringAsync();
                return Result<string, string>.Success(content);
            }
            catch (Exception error)
            {
                return Result<string, string>.Fail(error.ToString());
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
            var hostStr = url.Host;

            var lastDotIndex = hostStr.LastIndexOf('.');
            if (lastDotIndex < 0)
                return hostStr;

            var nextToLastDotIndex = hostStr.Substring(0, lastDotIndex).LastIndexOf('.');
            if (nextToLastDotIndex < 0)
                return hostStr;

            return hostStr.Substring(nextToLastDotIndex + 1, hostStr.Length - nextToLastDotIndex - 1);
        }

        private static string GetTooManyPagesErrorMessage(Uri url, int foundPagesCount, int pagesCountLimit)
        {
            return $"Не удалось проиндексировать сайт {url} из-за ограничения в {pagesCountLimit} страниц на сайт " +
                   $"(найдено не менее {foundPagesCount} страниц). Проиндексирована только главная страница.";
        }
    }
}
