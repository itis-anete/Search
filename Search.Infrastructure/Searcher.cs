namespace Search.Infrastructure
{
    public class Searcher
    {
        public Searcher(ISearchDatabase searchDatabase, ISearchCache searchCache = null)
        {
            _searchDatabase = searchDatabase;
            _searchCache = searchCache;
        }

        public SearchResponse Search(SearchRequest request)
        {
            if (_searchCache != null && _searchCache.TryGetResponse(request, out var response))
                return response;

            return _searchDatabase.Search(request);
        }

        public readonly ISearchDatabase _searchDatabase;
        private readonly ISearchCache _searchCache;
    }
}
