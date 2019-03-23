using Microsoft.Extensions.Configuration;
using Nest;
using Search.Core.Entities;
using System;
using System.Linq;

namespace Search.Infrastructure
{
    public class ElasticSearchDatabase : ISearchDatabase
    {
        public ElasticSearchDatabase(string elasticSearchUrl)
        {
            var uri = new Uri(elasticSearchUrl);
            var connectionSettings = new ConnectionSettings(uri)
                .DefaultMappingFor<DocumentInfo>(mapping => mapping
                    .IndexName(IndexName)
                    .IdProperty(x => x.Url)
                )
                .ThrowExceptions();

            _client = new ElasticClient(connectionSettings);

            EnsureIndexCreated();
        }

        public ElasticSearchDatabase(IConfiguration configuration)
            : this(configuration.GetValue<string>("ElasticSearchUrl")) { }

        public void Add(DocumentInfo document)
        {
            _client.IndexDocument(document);
        }

        public SearchResponse Search(SearchRequest request)
        {
            var response = _client.Search<DocumentInfo>(search => search
                .From(request.From)
                .Size(request.Size)
                .Query(query => query.
                    Match(match => match
                        .Field(x => x.Title)
                        .Query(request.Query)
                    ) || query
                    .Match(match => match
                        .Field(x => x.Text)
                        .Query(request.Query)
                    )
                )
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

        public void Remove(string url)
        {
            _client.Delete<DocumentInfo>(url);
        }

        private readonly ElasticClient _client;

        private const string IndexName = "search-dot-net_main_index";

        private void EnsureIndexCreated()
        {
            var existsResponse = _client.IndexExists(Indices.Index(IndexName));
            if (existsResponse.Exists)
                return;

            var createResponse = _client.CreateIndex(IndexName, index => index
                .Settings(settings => settings
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
                    )
                )
                .Mappings(mappings => mappings
                    .Map<DocumentInfo>(map => map
                        .Properties(properties => properties
                            .Text(property => property
                                .Name(x => x.Title)
                                .Analyzer("english_russian")
                                .Boost(3)
                            )
                            .Text(property => property
                                .Name(x => x.Text)
                                .Analyzer("english_russian")
                                .Store()
                            )
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }
    }
}
