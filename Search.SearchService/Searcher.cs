using Search.Infrastructure;
using System.Linq;

namespace Search.SearchService
{
    public class Searcher
    {
        public Searcher(ElasticSearchDatabase database, IRequestCache searchCache = null)
        {
            _database = database;
            _searchCache = searchCache;
        }

        public SearchResponse Search(SearchRequest request)
        {
            if (_searchCache != null && _searchCache.TryGetResponse(request, out var response))
                return response;

            var responseFromElastic = _database.Search(
                request.Query,
                request.From,
                request.Size);

            response = new SearchResponse
            {
                Results = responseFromElastic.Documents
                    .Select(document => new SearchResult
                    {
                        Url = document.Url,
                        Title = document.Title
                    })
                    .ToList()
            };

            _searchCache?.Add(request, response);
            return response;
        }

        private readonly ElasticSearchDatabase _database;
        private readonly IRequestCache _searchCache;
    }
}
