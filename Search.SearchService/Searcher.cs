using RailwayResults;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.Core.Extensions;
using System.Linq;
using System.Net;

namespace Search.SearchService
{
    public class Searcher
    {
        public Searcher(
            ElasticSearchClient<Document> client,
            ElasticSearchOptions options,
            IRequestCache searchCache = null)
        {
            _client = client;
            _options = options;
            _searchCache = searchCache;
        }

        public Result<SearchResponse, HttpStatusCode> Search(SearchRequest request)
        {
            if (_searchCache != null && _searchCache.TryGetResponse(request, out var response))
                return Result<SearchResponse, HttpStatusCode>.Success(response);

            var responseFromElastic = _client.Search(search => search
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
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<SearchResponse, Document>(
                    responseFromElastic, () =>
                    {
                        if (responseFromElastic.ServerError.Status == HttpStatusCode.NotFound.ToInt())
                            return HttpStatusCode.ServiceUnavailable;

                        return null;
                    }
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
            return Result<SearchResponse, HttpStatusCode>.Success(response);
        }

        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        private readonly IRequestCache _searchCache;
    }
}
