using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.IndexService;
using Search.Infrastructure;
using Search.SearchService;
using Search.VersioningService;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Search.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(new ElasticSearchOptions
            {
                Url = new Uri(Configuration.GetValue<string>("ElasticSearchUrl")),
                EnableVersioning = true
            });
            services.AddSingleton<ElasticSearchClient>();

            services.AddSingleton<Searcher>();
            services.AddSingleton<IRequestCache, MemoryRequestCache>();

            services.AddSingleton<Indexer>();

            services.AddSingleton<VersionsSearcher>();

            services.AddSingleton<ServiceContainer>();

            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("ApiList", new Info {
                    Title = "Search API",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseSwagger(x =>
            {
                x.RouteTemplate = "/api/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/api/swagger/ApiList/swagger.json", "");
                x.RoutePrefix = "api/swagger";
                x.DocumentTitle = "Search API";
                x.DisplayRequestDuration();
            });

            app.UseMvc(routes =>
            {
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
