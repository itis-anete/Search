using Nest;
using Search.Core.Database;
using Search.Core.Entities;
using System;
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
                    .Map<Document>(map => map
                        .Properties(properties => properties
                            .Text(ElasticSearchOptions.TitleProperty)
                            .Text(ElasticSearchOptions.TextProperty)
                            .Date(ElasticSearchOptions.IndexedTimeProperty)
                            .Keyword(ElasticSearchOptions.UrlProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }

        private void ElasticSearchClient_OnIndex(
            Document document,
            IIndexRequest<Document> request,
            IIndexResponse response)
        {
            if (request.Index != _options.DocumentsIndexName)
                return;

            var latestDocument = GetLatestDocumentWithUrl(document.Url);
            if (document.Title == latestDocument?.Title &&
                document.Text == latestDocument?.Text)
                return;

            _client.Index(document, desc => desc
                .Index(_options.VersionsIndexName));
        }

        private Document GetLatestDocumentWithUrl(Uri url)
        {
            var searchResponse = _client.Search(desc => desc
                .Index(_options.VersionsIndexName)
                .Query(query => query
                    .Term(term => term
                        .Field(x => x.Url)
                        .Value(url)
                    )
                )
            );
            var oldDocuments = searchResponse.Hits;
            if (oldDocuments.Count == 0)
                return null;

            var latestDocumentHit = oldDocuments.Aggregate((x, y) =>
                x.Source.IndexedTime > y.Source.IndexedTime ? x : y);
            var latestDocument = latestDocumentHit.Source;

            var getResponse = _client.Get(
                DocumentPath<Document>.Id(latestDocumentHit.Id),
                desc => desc
                    .Index(_options.VersionsIndexName)
                    .StoredFields(x => x.Text)
            );

            latestDocument.Text = getResponse.Fields["text"].As<string[]>().Single();
            return latestDocument;
        }
    }
}
