using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Plugins.AzureServiceBus;

namespace Weikio.EventFramework.Samples.AzureServiceBus
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
            services.AddCloudEventSources();
            services.AddCloudEventPublisher();
            services.AddCloudEventDataflowChannels();
            
            Directory.CreateDirectory(@"c:\temp\myevents");
            services.AddChannel("bus", (provider, options) =>
            {
                options.Endpoints.Add(new CloudEventsEndpoint(async ev =>
                {
                    var outputPath = Path.Combine(@"c:\temp\myevents", "bus_" + ev.Id.ToString() + ".json");

                    await File.WriteAllTextAsync(outputPath, ev.ToJson());
                }));

                var azureServiceBusEndpoint = new AzureServiceBusEndpoint(new AzureServiceBusConfiguration()
                {
                    QueueName = "bus_output"
                }, provider.GetRequiredService<ILogger<AzureServiceBusEndpoint>>());
                
                options.Endpoints.Add((azureServiceBusEndpoint.Send, null));
            });
            
            services.AddChannel("bus2", (provider, options) =>
            {
                options.Endpoints.Add(new CloudEventsEndpoint(async ev =>
                {
                    var outputPath = Path.Combine(@"c:\temp\myevents", "bus2_" + ev.Id.ToString() + ".json");

                    await File.WriteAllTextAsync(outputPath, ev.ToJson());
                }));
            });

            services.AddEventSource<AzureServiceBusEventSource>(options =>
            {
                options.Configuration = new AzureServiceBusConfiguration()
                {
                    QueueName = "bus",
                };

                options.Autostart = true;
                options.TargetChannelName = "bus";
            });
            
            services.AddEventSource<AzureServiceBusEventSource>(options =>
            {
                options.Configuration = new AzureServiceBusConfiguration()
                {
                    QueueName = "bus_output",
                };

                options.Autostart = true;
                options.TargetChannelName = "bus2";
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
