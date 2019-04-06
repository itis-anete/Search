using Nest;
using Search.Core.Entities;
using Search.Infrastructure;
using Search.SearchService.Internal;
using System.Collections.Generic;

namespace Search.SearchService
{
    public class MemoryRequestCache : IRequestCache
    {
        public MemoryRequestCache(ElasticSearchClient client, ElasticSearchOptions options)
        {
            _options = options;

            client.OnIndex += ElasticSearchClient_OnIndex;
        }

        public void Add(SearchRequest request, SearchResponse response)
        {
            _cache[request] = response;
        }

        public SearchResponse GetResponse(SearchRequest request)
        {
            if (_cache.TryGetValue(request, out var response))
                return response;
            return null;
        }

        public bool IsCached(SearchRequest request)
        {
            return _cache.ContainsKey(request);
        }

        public void Remove(SearchRequest request)
        {
            _cache.Remove(request);
        }

        public bool TryGetResponse(SearchRequest request, out SearchResponse response)
        {
            return _cache.TryGetValue(request, out response);
        }

        private readonly ElasticSearchOptions _options;
        private readonly Dictionary<SearchRequest, SearchResponse> _cache =
                     new Dictionary<SearchRequest, SearchResponse>(Comparer);

        private static readonly RequestComparer Comparer = new RequestComparer();

        private void ElasticSearchClient_OnIndex(
            DocumentInfo document,
            IIndexRequest<DocumentInfo> request,
            IIndexResponse response)
        {
            if (request.Index != _options.DocumentsIndexName)
                return;

            _cache.Clear();
        }
    }
}
