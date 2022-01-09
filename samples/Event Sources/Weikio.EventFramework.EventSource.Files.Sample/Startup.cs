using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace Weikio.EventFramework.EventSource.Files.Sample
{
    public class Startup
    {
        public const string FolderToMonitor = @"c:\temp\listen";
        public const string FolderToMonitorForCloudEvents = @"c:\temp\listencloudevents";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddEventFramework()
                .AddFileEventSource(options =>
                {
                    options.Configuration = new FileSystemEventSourceConfiguration() { Filter = "*.txt", Folder = FolderToMonitor };
                    options.Autostart = true;
                    options.Id = "mylocalfiles";
                })
                .AddChannel("local", (provider, options) =>
                {
                    var aggregator = provider.GetRequiredService<ICloudEventAggregator>();

                    var endpoint = new CloudEventsEndpoint(async ev =>
                    {
                        await aggregator.Publish(ev);
                    });

                    options.Endpoints.Add(endpoint);
                })
                .AddHandler(ev =>
                {
                    var json = ev.ToJson();
                    Console.WriteLine(json);
                })
                .AddEventFlow(EventFlowBuilder.From<FileCloudEventSource>(options =>
                    {
                        options.Id = "fileev";
                        options.Autostart = true;
                        options.Configuration = new FileCloudEventSourceConfiguration() { Folder = FolderToMonitorForCloudEvents, IncludeSubfolders = false };
                    })
                    .Handle(ev =>
                    {
                        var json = ev.ToJson();
                        Console.WriteLine(json);
                    }));

            services.Configure<DefaultChannelOptions>(options =>
            {
                options.DefaultChannelName = "local";
            });
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
