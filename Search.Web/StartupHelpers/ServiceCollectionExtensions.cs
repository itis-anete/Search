using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.FearchFervice;
using Search.IndexHelpers;
using Search.IndexService;
using Search.IndexService.Dto;
using Search.IndexService.SiteMap;

namespace Search.Web.StartupHelpers
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
            services.AddSingleton<HangingRequestsHandler>();
            services.AddHostedService<HangingRequestsHandlerAdapter>();
            
            services.AddHostedService<Reindexer>();
            
            services.AddSingleton<SiteMapGetter>();
            services.AddSingleton<SiteMapIndex>();
            services.AddSingleton<PagesPerSiteLimiter>();
            services.AddHostedService<Indexer>();
        }

        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<Fearcher>();

            services.AddSingleton<QueueForIndex>();
        }

        public static void ConfigureHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            var headers = new Dictionary<string, string>();
            configuration
                .GetSection("HttpClientConfig")
                .Bind("Headers", headers);
            services.AddHttpClient("Page downloader", client =>
            {
                headers.ForEach(h =>
                    client.DefaultRequestHeaders.Add(h.Key, h.Value));
            });
        }

        public static void ConfigureAllOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PagesPerSiteLimiterOptions>(configuration);
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
