using Search.Infrastructure;

namespace Search.SearchService
{
    public interface ISearchCache
    {
        bool IsCached(SearchRequest request);

        SearchResponse GetResponse(SearchRequest request);
        bool TryGetResponse(SearchRequest request, out SearchResponse response);

        void Add(SearchRequest request, SearchResponse response);
        void Remove(SearchRequest request);
    }
}
