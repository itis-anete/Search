using Nest;
using Search.Core.Entities;

namespace Search.Infrastructure
{
    public class ElasticClientBuilder
    {
        public ElasticClientBuilder UseOptions(ElasticSearchOptions options)
        {
            _options = options;
            return this;
        }

        public ElasticClient Build()
        {
            var connectionSettings = new ConnectionSettings(_options.Url)
                .DefaultMappingFor<DocumentInfo>(mapping => mapping
                    .IndexName(_options.DocumentIndexName)
                    .IdProperty(x => x.Url)
                )
                .ThrowExceptions();

            var client = new ElasticClient(connectionSettings);
            EnsureIndexCreated(client);
            return client;
        }

        private ElasticSearchOptions _options;

        private void EnsureIndexCreated(ElasticClient client)
        {
            var response = client.IndexExists(Indices.Index(_options.DocumentIndexName));
            if (response.Exists)
                return;

            ITextProperty TitleProperty(TextPropertyDescriptor<DocumentInfo> property) => property
                .Name(x => x.Title)
                .Analyzer("english_russian")
                .Boost(3);

            ITextProperty TextProperty(TextPropertyDescriptor<DocumentInfo> property) => property
                .Name(x => x.Text)
                .Analyzer("english_russian")
                .Store();

            client.CreateIndex(_options.DocumentIndexName, index => index
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
                            .Text(TitleProperty)
                            .Text(TextProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }
    }
}
