using Search.Infrastructure;
using System.Linq;

namespace Search.SearchService
{
    public class Searcher
    {
        public Searcher(
            ElasticSearchClient database,
            ElasticSearchOptions options,
            IRequestCache searchCache = null)
        {
            _database = database;
            _options = options;
            _searchCache = searchCache;
        }

        public SearchResponse Search(SearchRequest request)
        {
            if (_searchCache != null && _searchCache.TryGetResponse(request, out var response))
                return response;

            var responseFromElastic = _database.Search(search => search
                .Index(_options.DocumentsIndexName)
                .From(request.From)
                .Size(request.Size)
                .Query(desc => desc
                    .Match(match => match
                        .Field(x => x.Title)
                        .Query(request.Query)
                    ) || desc
                    .Match(match => match
                        .Field(x => x.Text)
                        .Query(request.Query)
                    )
                )
            );

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

        private readonly ElasticSearchClient _database;
        private readonly ElasticSearchOptions _options;
        private readonly IRequestCache _searchCache;
    }
}
