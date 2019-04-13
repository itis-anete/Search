using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.IndexService;
using Search.Infrastructure;
using Search.SearchService;
using Search.VersioningService;
using System;

namespace Search.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticSearchUrl = configuration.GetValue<string>("ElasticSearchUrl");
            services.AddSingleton(new ElasticSearchOptions
            {
                Url = new Uri(elasticSearchUrl)
            });

            services.AddSingleton<ElasticSearchClient>();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<Searcher>();
            services.AddSingleton<IRequestCache, MemoryRequestCache>();

            services.AddSingleton<Indexer>();

            services.AddSingleton<VersionsSearcher>();

            services.AddSingleton<ServiceContainer>();
        }
    }
}
