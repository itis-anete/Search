namespace Search.Infrastructure
{
    public class Searcher
    {
        public Searcher(ISearchDatabase searchDatabase)
        {
            _searchDatabase = searchDatabase;
        }

        public SearchResponse Search(SearchRequest request)
        {
            return _searchDatabase.Search(request);
        }

        public readonly ISearchDatabase _searchDatabase;
    }
}
