using Nest;
using Search.Core.Entities;

namespace Search.Infrastructure
{
    public class ElasticSearchDatabase
    {
        public ElasticSearchDatabase(ElasticSearchOptions options)
        {
            var connectionSettings = new ConnectionSettings(options.Url)
                .ThrowExceptions();

            _client = new ElasticClient(connectionSettings);
            EnsureDocumentIndexCreated(_client, options);

            if (options.EnableVersioning)
                EnsureDocumentHistoryIndexCreated(_client, options);

            _options = options;
        }

        public void Add(DocumentInfo document)
        {
            _client.Index(document, request => request
                .Id(document.Url.ToString())
                .Index(_options.DocumentsIndexName));
            _client.Index(document, request => request
                .Index(_options.VersionsIndexName));
        }

        public ISearchResponse<DocumentInfo> Search(
            string query,
            int? from = null,
            int? size = null)
        {
            return _client.Search<DocumentInfo>(search => search
                .Index(_options.DocumentsIndexName)
                .From(from)
                .Size(size)
                .Query(desc => desc
                    .Match(match => match
                        .Field(x => x.Title)
                        .Query(query)
                    ) || desc
                    .Match(match => match
                        .Field(x => x.Text)
                        .Query(query)
                    )
                )
            );
        }

        public ISearchResponse<DocumentInfo> SearchInHistory(
           string query,
           int? from = null,
           int? size = null)
        {
            return _client.Search<DocumentInfo>(search => search
                .Index(_options.VersionsIndexName)
                .From(from)
                .Size(size)
                .Query(desc => desc
                    .Match(match => match
                        .Field(x => x.Title)
                        .Query(query)
                    ) || desc
                    .Match(match => match
                        .Field(x => x.Text)
                        .Query(query)
                    )
                )
            );
        }

        public void Remove(string url)
        {
            _client.Delete<DocumentInfo>(url, desc => desc
                .Index(_options.DocumentsIndexName));
        }

        private readonly ElasticClient _client;
        private readonly ElasticSearchOptions _options;

        private static ITextProperty TitleProperty(TextPropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.Title)
            .Analyzer("english_russian")
            .Boost(3);

        private static ITextProperty TextProperty(TextPropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.Text)
            .Analyzer("english_russian")
            .Store();

        private static IDateProperty IndexedTimeProperty(DatePropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.IndexedTime);

        private static IPromise<IIndexSettings> AnalysisSettings(IndexSettingsDescriptor settings) => settings
            .Analysis(analysis => analysis
                .TokenFilters(filters => filters
                    .Stop("stop", stop => stop.StopWords("_english_", "_russian_"))
                    .Stemmer("english_stemmer", stemmer => stemmer.Language("english"))
                    .Stemmer("english_possessive_stemmer", stemmer => stemmer.Language("possessive_english"))
                    .Stemmer("russian_stemmer", stemmer => stemmer.Language("russian"))
                )
                .Analyzers(analyzers => analyzers
                    .Custom("english_russian", analyzer => analyzer
                        .Tokenizer("standard")
                        .Filters("lowercase", "stop", "english_stemmer", "english_possessive_stemmer", "russian_stemmer")
                    )
                )
            );

        private static void EnsureDocumentIndexCreated(ElasticClient client, ElasticSearchOptions options)
        {
            var response = client.IndexExists(options.DocumentsIndexName);
            if (response.Exists)
                return;

            client.CreateIndex(options.DocumentsIndexName, index => index
                .Settings(AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<DocumentInfo>(map => map
                        .Properties(properties => properties
                            .Text(TitleProperty)
                            .Text(TextProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }

        private static void EnsureDocumentHistoryIndexCreated(ElasticClient client, ElasticSearchOptions options)
        {
            var response = client.IndexExists(options.VersionsIndexName);
            if (response.Exists)
                return;

            client.CreateIndex(options.VersionsIndexName, index => index
                .Settings(AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<DocumentInfo>(map => map
                        .Properties(properties => properties
                            .Text(TitleProperty)
                            .Text(TextProperty)
                            .Date(IndexedTimeProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }
    }
}
