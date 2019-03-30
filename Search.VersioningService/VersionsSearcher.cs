using Search.Infrastructure;
using System.Linq;

namespace Search.VersioningService
{
    public class VersionsSearcher
    {
        public VersionsSearcher(ElasticSearchDatabase database)
        {
            _database = database;
        }

        public VersionsSearchResponse Search(VersionsSearchRequest request)
        {
            var responseFromElastic = _database.SearchInHistory(
                request.Query
            );

            return new VersionsSearchResponse
            {
                Results = responseFromElastic.Documents
                    .Select(document => new VersionsSearchResult
                    {
                        Url = document.Url,
                        IndexedTime = document.IndexedTime,
                        Title = document.Title
                    })
                    .ToList()
            };
        }

        private readonly ElasticSearchDatabase _database;
    }
}
