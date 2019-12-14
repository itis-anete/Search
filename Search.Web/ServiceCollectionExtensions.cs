using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService;
using Search.IndexService.Dto;
using Search.SearchService;
using System;

namespace Search.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticSearchUrl = GetElasticSearchUrl(configuration);
            services.AddSingleton(new ElasticSearchOptions
            {
                Url = new Uri(elasticSearchUrl)
            });

            services.AddSingleton<ElasticSearchClient<IndexRequestDto>>();
            services.AddSingleton<ElasticSearchClient<Document>>();
        }

        public static void AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<Indexer>();
            services.AddHostedService<Reindexer>();
        }

        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<Searcher>();
            services.AddSingleton<IRequestCache, MemoryRequestCache>();

            services.AddSingleton<QueueForIndex>();

            services.AddSingleton<ServiceContainer>();
        }

        private static string GetElasticSearchUrl(IConfiguration configuration)
        {
            var url = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
            if (!string.IsNullOrEmpty(url))
                return url;

            url = configuration.GetValue<string>("ElasticSearchUrl");
            return url;
        }
    }
}
