using Nest;
using Search.Core.Entities;
using Search.Infrastructure;
using System.Linq;

namespace Search.VersioningService
{
    public class VersionsSearcher
    {
        public VersionsSearcher(ElasticSearchClient client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;

            EnsureIndexCreated();
            _client.OnIndex += ElasticSearchClient_OnIndex;
        }

        public VersionsSearchResponse Search(VersionsSearchRequest request)
        {
            var response = _client.Search(search => search
                .Index(_options.VersionsIndexName)
                .From(request.From)
                .Size(request.Size)
                .Query(desc =>
                    (
                        desc.Match(match => match
                            .Field(x => x.Title)
                            .Query(request.Query)
                        )
                        ||
                        desc.Match(match => match
                            .Field(x => x.Text)
                            .Query(request.Query)
                        )
                    )
                    &&
                    desc.DateRange(range =>
                    {
                        if (request.FromDate != null)
                            range = range.GreaterThanOrEquals(request.FromDate);
                        if (request.ToDate != null)
                            range = range.LessThanOrEquals(request.ToDate);
                        return range.Field(x => x.IndexedTime);
                    })
                )
            );

            return new VersionsSearchResponse
            {
                Results = response.Documents
                    .Select(document => new VersionsSearchResult
                    {
                        Url = document.Url,
                        IndexedTime = document.IndexedTime,
                        Title = document.Title
                    })
                    .ToList()
            };
        }

        private readonly ElasticSearchClient _client;
        private readonly ElasticSearchOptions _options;

        private void EnsureIndexCreated()
        {
            var response = _client.IndexExists(_options.VersionsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.VersionsIndexName, index => index
                .Settings(ElasticSearchOptions.AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<DocumentInfo>(map => map
                        .Properties(properties => properties
                            .Text(ElasticSearchOptions.TitleProperty)
                            .Text(ElasticSearchOptions.TextProperty)
                            .Date(ElasticSearchOptions.IndexedTimeProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }

        private void ElasticSearchClient_OnIndex(
            DocumentInfo document,
            IIndexRequest<DocumentInfo> request,
            IIndexResponse response)
        {
            if (request.Index != _options.DocumentsIndexName)
                return;

            _client.Index(document, desc => desc
                .Index(_options.VersionsIndexName));
        }
    }
}
