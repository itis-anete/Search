using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Search.Web.StartupHelpers;

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
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.ConfigureAllOptions(Configuration);

            services.ConfigureHttpClient(Configuration);
            services.AddElasticSearch(Configuration);
            services.AddDomainServices();

            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("ApiList", new OpenApiInfo {
                    Title = "Search API",
                    Version = "v1"
                });
            });
            
            services.AddBackgroundServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapDefaultControllerRoute();
            });
        }
    }
}
