using Search.Infrastructure;
using System.Collections.Generic;

namespace Search.SearchService
{
    public class MemorySearchCache : ISearchCache
    {
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

        private readonly Dictionary<SearchRequest, SearchResponse> _cache =
                     new Dictionary<SearchRequest, SearchResponse>(Comparer);

        private class RequestComparer : IEqualityComparer<SearchRequest>
        {
            public bool Equals(SearchRequest x, SearchRequest y)
            {
                return Equals(x.From, y.From)
                    && Equals(x.Size, y.Size)
                    && Equals(x.Query, y.Query);
            }

            public int GetHashCode(SearchRequest obj)
            {
                return unchecked(
                    obj.Query.GetHashCode() * 2081561 +
                    obj.From.GetHashCode() * 61583 +
                    obj.Size.GetHashCode());
            }
        }

        private static readonly RequestComparer Comparer = new RequestComparer();
    }
}
