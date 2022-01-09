using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventSource.Files;

namespace Weikio.EventFramework.Components.Security.Sample
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
            services.AddControllers();

            var fileToWebFlow = EventFlowBuilder.From<FileSystemEventSource>(options =>
                {
                    options.Autostart = true;
                    options.Configuration = new FileSystemEventSourceConfiguration() { Folder = @"c:\temp\listen" };
                })
                .Encrypt(publicKeyPath: "public.key")
                .Http("https://webhook.site/0633b58b-2eab-4875-a1f4-609264847c4d");
            
            services.AddEventFramework()
                .AddEventFlow(fileToWebFlow);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
