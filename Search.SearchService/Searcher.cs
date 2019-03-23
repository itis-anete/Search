using Nest;
using Search.Core.Entities;
using System.Linq;

namespace Search.SearchService
{
    public class Searcher
    {
        public Searcher(ElasticClient client, IRequestCache searchCache = null)
        {
            _client = client;
            _searchCache = searchCache;
        }

        public SearchResponse Search(SearchRequest request)
        {
            if (_searchCache != null && _searchCache.TryGetResponse(request, out var response))
                return response;

            response = SendRequest(request);
            _searchCache?.Add(request, response);
            return response;
        }

        private readonly ElasticClient _client;
        private readonly IRequestCache _searchCache;

        private SearchResponse SendRequest(SearchRequest request)
        {
            QueryContainer MatchTitleOrText(QueryContainerDescriptor<DocumentInfo> query) => query.
                Match(match => match
                    .Field(x => x.Title)
                    .Query(request.Query)
                ) || query
                .Match(match => match
                    .Field(x => x.Text)
                    .Query(request.Query));

            var response = _client.Search<DocumentInfo>(search => search
                .From(request.From)
                .Size(request.Size)
                .Query(MatchTitleOrText)
            );

            return new SearchResponse
            {
                Results = response.Documents
                    .Select(document => new SearchResult
                    {
                        Url = document.Url,
                        Title = document.Title
                    })
                    .ToList()
            };
        }
    }
}
