using Nest;
using Search.Core.Entities;
using Search.SearchService;
using System.Linq;

namespace Search.DataHistoryService
{
    public class CacheSearcher
    {
        public CacheSearcher(ElasticClient client)
        {
            _client = client;
        }

        public CacheSearchResponse Search(CacheSearchRequest request)
        {
            QueryContainer MatchTitleOrText(QueryContainerDescriptor<DocumentInfo> query) => query.
                   Match(match => match
                       .Field(x => x.Title)
                       .Query(request.Query)
                   ) || query
                   .Match(match => match
                       .Field(x => x.Text)
                       .Query(request.Query));

            var response = _client.RollupSearch<DocumentInfo>(Indices.All, search => search
                //.From(request.From)
                .Size(request.Size)
                .Query(MatchTitleOrText)
            );

            return new CacheSearchResponse
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

        private readonly ElasticClient _client;
    }
}
