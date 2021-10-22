using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Adafy.Candy.CosmosDB;
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
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.DataSources.CosmosDB;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace Weikio.EventFramework.Samples.EventSource
{
    public class Startup
    {
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
                .AddLocal()
                .AddHandler(ev =>
                {
                    var json = ev.ToJson();
                    Console.WriteLine(json);
                })
                .AddEventSource<TestEventSource>();
            
            // services.AddSingleton(new EventSourceInstanceOptions()
            // {
            //     Id = "nonpersist",
            //     Autostart = true, 
            //     PollingFrequency = TimeSpan.FromSeconds(1), 
            //     EventSourceDefinition = "TestEventSource",
            //     PersistState = false
            // });
            //
            // services.AddSingleton(new EventSourceInstanceOptions()
            // {
            //     Id = "persist",
            //     Autostart = true, 
            //     PollingFrequency = TimeSpan.FromSeconds(1), 
            //     EventSourceDefinition = "TestEventSource",
            //     PersistState = true
            // });
            //
            // services.AddSingleton(new EventSourceInstanceOptions()
            // {
            //     Id = "anotherpersist",
            //     Autostart = true, 
            //     PollingFrequency = TimeSpan.FromSeconds(1), 
            //     EventSourceDefinition = "TestEventSource",
            //     PersistState = true
            // });
            //
            services.AddSingleton(new EventSourceInstanceOptions()
            {
                Id = "third",
                Autostart = true, 
                PollingFrequency = TimeSpan.FromSeconds(1), 
                EventSourceDefinition = "TestEventSource",
                PersistState = true,
                EventSourceInstanceDataStoreType = typeof(CosmosDBEventSourceInstanceDataSource)
            });
            
            services.Configure<DefaultChannelOptions>(options =>
            {
                options.DefaultChannelName = "bus";
            });

            services.AddCosmosDbStateStore(settings =>
            {
                settings.CollectionId = "efstate";
                settings.DatabaseId = "myefdb";
                settings.DocumentDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                settings.DocumentDbUri = "https://localhost:8081";
                settings.IsDefaultStateStore = false;
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
