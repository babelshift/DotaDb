using DotaDb.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Azure;
using SourceSchemaParser.Utilities;
using DotaDb.Models;
using AutoMapper;

namespace DotaDb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configSection = Configuration.GetSection("BlobStorage");
            var storageConnectionString = configSection.Value;
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSourceSchemaParser();
            services.AddAzureClients(builder => {
                builder.AddBlobServiceClient(storageConnectionString);
            });
            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddSingleton<BlogFeedService>();
            services.AddSingleton<PlayerCountService>();
            services.AddSingleton<LiveLeagueGamesService>();
            services.AddSingleton<BlobStorageService>();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<HeroService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}