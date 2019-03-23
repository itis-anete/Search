using Microsoft.Extensions.Configuration;
using Nest;
using Search.Core.Entities;
using System;

namespace Search.Infrastructure
{
    public class ElasticClientBuilder
    {
        public ElasticClientBuilder SetUrl(Uri url)
        {
            _url = url;
            return this;
        }

        public ElasticClientBuilder SetUrl(string url)
        {
            _url = new Uri(url);
            return this;
        }

        public ElasticClientBuilder UseConfiguration(IConfiguration configuration)
        {
            var url = configuration.GetValue<string>("ElasticSearchUrl");
            SetUrl(url);

            return this;
        }

        public ElasticClientBuilder EnableHistorySearch(bool enabled = true)
        {
            _historySearchEnabled = enabled;
            return this;
        }

        public ElasticClient Build()
        {
            var connectionSettings = new ConnectionSettings(_url)
                .DefaultMappingFor<DocumentInfo>(mapping => mapping
                    .IndexName(DocumentIndexName)
                    .IdProperty(x => x.Url)
                )
                .ThrowExceptions();

            var client = new ElasticClient(connectionSettings);
            EnsureIndexCreated(client);

            if (_historySearchEnabled)
                EnsureRollupJobStarted(client);

            return client;
        }

        private Uri _url;
        private bool _historySearchEnabled;

        private const string DocumentIndexName = "search-dot-net_main_index";

        private static void EnsureIndexCreated(ElasticClient client)
        {
            var response = client.IndexExists(Indices.Index(DocumentIndexName));
            if (response.Exists)
                return;

            client.CreateIndex(DocumentIndexName, index => index
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

        private static void EnsureRollupJobStarted(ElasticClient client)
        {
            const string JobName = "search-dot-net_rollup_job";
            const string RollupIndexName = "search-dot-net_rollup_index";

            var response = client.GetRollupJob(job => job.Id(JobName));
            if (response.Jobs.Count > 0)
                return;

            IPromise<Fields> FieldsToSave(FieldsDescriptor<DocumentInfo> fields) => fields
                .Fields(
                    x => x.Url,
                    x => x.Title,
                    x => x.Text
                );

            client.CreateRollupJob<DocumentInfo>(JobName, job => job
                .IndexPattern(DocumentIndexName)
                .RollupIndex(RollupIndexName)
                .Cron("*/30 * * * * ?")
                .PageSize(1000)
                .Groups(groups => groups
                    .DateHistogram(date => date
                        //.Field(x => x)
                        .Interval(TimeSpan.FromMinutes(20))
                    )
                    .Terms(terms => terms.Fields(FieldsToSave))
                )
            );
            client.StartRollupJob(JobName);
        }
    }
}
