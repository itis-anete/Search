using Microsoft.Extensions.Configuration;
using Nest;
using Search.Core.Entities;
using System;
using System.Linq;

namespace Search.Infrastructure.Implementation
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
                    .Ignore(x => x.Text)
                );

            _client = new ElasticClient(connectionSettings);

            EnsureIndexCreated();
        }

        public ElasticSearchDatabase(IConfiguration configuration)
            : this(configuration.GetValue<string>("ElasticSearchUrl")) { }

        public void Add(DocumentInfo document)
        {
            var response = _client.IndexDocument(document);
            ThrowIfNotValid(response);
        }

        public SearchResponse Search(SearchRequest request)
        {
            var response = _client.Search<DocumentInfo>(search => search
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
            ThrowIfNotValid(response);

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
            var response = _client.Delete<DocumentInfo>(url);
            ThrowIfNotValid(response);
        }

        private readonly ElasticClient _client;
        private const string IndexName = "search-dot-net_main_index";

        private void EnsureIndexCreated()
        {
            var existsResponse = _client.IndexExists(Indices.Index(IndexName));
            ThrowIfNotValid(existsResponse);

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
                            .Text(text => text
                                .Name(x => x.Title)
                                .Analyzer("english_russian")
                                .Boost(3)
                            )
                            .Text(text => text
                                .Name(x => x.Text)
                                .Analyzer("english_russian")
                                .Store()
                            )
                        )
                    )
                )
            );
            ThrowIfNotValid(createResponse);
        }

        private static void ThrowIfNotValid(IResponse response)
        {
            if (!response.IsValid)
                throw response.OriginalException;
        }
    }
}
