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
using Weikio.EventFramework.EventSource.Http;

namespace Weikio.EventFramework.Components.File.Sample
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
            var httpToFileFlow = EventFlowBuilder.From<HttpCloudEventEventSource>(options =>
                {
                    options.Autostart = true;
                    options.Configuration = new HttpCloudEventReceiverApiConfiguration() { Route = "/incoming" };
                })
                .File(@"c:\temp\incominghttp");

            var fileToLoggerFlow = EventFlowBuilder.From<FileCloudEventSource>(options =>
                {
                    options.Autostart = true;
                    options.Configuration = new FileCloudEventSourceConfiguration() { Folder = @"c:\temp\incominghttp" };
                    options.Id = "localfiles";
                })
                .Logger();
                
            services.AddControllers();

            services.AddEventFramework()
                .AddEventFlow(httpToFileFlow)
                .AddEventFlow(fileToLoggerFlow);
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
