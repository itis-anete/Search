using Nest;
using Search.Core.Database;
using Search.Core.Entities;
using Search.SearchService.Internal;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Search.SearchService
{
    public sealed class MemoryRequestCache : IRequestCache, IDisposable
    {
        public MemoryRequestCache(
            ElasticSearchClient client,
            ElasticSearchOptions options,
            TimeSpan? maxRecordAge = null)
        {
            _options = options;
            _maxRecordAge = maxRecordAge ?? TimeSpan.FromMinutes(5);

            client.OnIndex += ClearOnIndexing;

            var ageCheckFrequency = _maxRecordAge / 2;
            _ageStopwatch = Stopwatch.StartNew();
            _ageCheckingTimer = new Timer(
                CheckAgeOfAllRecords,
                null,
                ageCheckFrequency,
                ageCheckFrequency);
        }

        public void Add(SearchRequest request, SearchResponse response)
        {
            _cache[request] = new CacheRecord(response, _ageStopwatch.Elapsed);
        }

        public SearchResponse GetResponse(SearchRequest request)
        {
            if (_cache.TryGetValue(request, out var record))
                return record.Response;
            return null;
        }

        public bool IsCached(SearchRequest request)
        {
            CheckRecordAge(request, resetAge: true);
            return _cache.ContainsKey(request);
        }

        public void Remove(SearchRequest request)
        {
            _cache.TryRemove(request, out var record);
        }

        public bool TryGetResponse(SearchRequest request, out SearchResponse response)
        {
            response = null;
            CheckRecordAge(request, resetAge: true);

            if (!_cache.TryGetValue(request, out var record))
                return false;

            response = record.Response;
            return true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _ageCheckingTimer.Dispose();
            _cache.Clear();
            _ageStopwatch.Stop();

            _disposed = true;
        }

        private readonly TimeSpan _maxRecordAge;
        private readonly ElasticSearchOptions _options;

        private readonly ConcurrentDictionary<SearchRequest, CacheRecord> _cache =
                     new ConcurrentDictionary<SearchRequest, CacheRecord>(Comparer);
        private readonly Stopwatch _ageStopwatch;
        private readonly Timer _ageCheckingTimer;
        private bool _disposed;

        private static readonly RequestComparer Comparer = new RequestComparer();

        private void ClearOnIndexing(
            Document document,
            IIndexRequest<Document> request,
            IIndexResponse response)
        {
            if (request.Index != _options.DocumentsIndexName)
                return;

            _cache.Clear();
        }

        private void CheckAgeOfAllRecords(object state)
        {
            var keys = _cache.Keys.ToArray();
            foreach (var request in keys)
                CheckRecordAge(request);
        }

        private void CheckRecordAge(SearchRequest request, bool resetAge = false)
        {
            if (!_cache.TryGetValue(request, out var record))
                return;

            var currentAge = _ageStopwatch.Elapsed - record.CreationTime;
            if (currentAge > _maxRecordAge)
                _cache.TryRemove(request, out record);
            else if (resetAge)
                record.CreationTime = _ageStopwatch.Elapsed;
        }
    }
}
