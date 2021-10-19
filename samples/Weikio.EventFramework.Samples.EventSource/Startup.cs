using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.AspNetCore;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Plugins.Files;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace Weikio.EventFramework.Samples.EventSource
{
    public static class DataStore
    {
        public static List<CloudEvent> Events { get; set; } = new List<CloudEvent>();
    }

    public class WebPageEventSource
    {
        public async Task<(ContentChanged NewEvent, string NewState)> CheckForUpdatedWebSite(string currentState)
        {
            var client = new HttpClient();

            var result = await client.GetStringAsync(
                "https://fi.newbalance.eu/on/demandware.store/Sites-newbalance_eu-Site/en/Product-GetVariants?pid=MRCELV1-34439");

            if (string.Equals(currentState, result))
            {
                return (null, result);
            }

            return (new ContentChanged() { Content = result }, result);
        }
    }

    public class ContentChanged
    {
        public string Content { get; set; }
    }

    public class MegaHandler
    {
        private readonly ILogger<MegaHandler> _logger;

        public MegaHandler(ILogger<MegaHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CloudEvent<ContentChanged> contentChangedEvent)
        {
            _logger.LogInformation("Received event {CloudEvent}", contentChangedEvent);
            
            return Task.CompletedTask;
        }
    }
    
    public class Handler
    {
        public Task Handle(CloudEvent<ContentChanged> contentChangedEvent)
        {
            return Task.CompletedTask;
        }
    }
    public class Startup
    {
        public const string FolderToMonitor = @"c:\temp\listen";
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddEventFramework()
                .AddCloudEventSources()
                .AddCloudEventAggregator()
                .AddChannel("bus", (provider, options) =>
                {
                    options.Endpoint = async ev =>
                    {
                        var aggr = provider.GetRequiredService<ICloudEventAggregator>();
                        await aggr.Publish(ev);
                    };
                })
                // .AddFileEventSource(options =>
                // {
                //     options.Configuration = new FileEventSourceConfiguration() { Filter = "*.txt", Folder = FolderToMonitor };
                //     options.Autostart = true;
                // })
                .AddLocal()
                .AddHandler(ev =>
                {
                    var json = ev.ToJson();
                    Console.WriteLine(json);
                })
                .AddEventSource<TestEventSource>(options =>
                {
                    options.Autostart = true;
                    options.Id = "mytest";
                    options.PollingFrequency = TimeSpan.FromSeconds(10);
                });
            
            services.Configure<DefaultChannelOptions>(options =>
            {
                options.DefaultChannelName = "bus";
            });
        }

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
