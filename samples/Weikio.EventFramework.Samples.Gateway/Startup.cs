using System.Collections.Generic;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventGateway.Http;

namespace Weikio.EventFramework.Samples.Gateway
{
    public static class DataStore
    {
        public static List<CloudEvent> Events { get; set; } = new List<CloudEvent>();
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // 1. Create a Http Gateway which can accept CloudEvents. By default this accepts messages at /api/events

            // 2. Configure the gateway to write all the received events into the Datastore
            services.Configure<CloudEventGatewayOptions>(options =>
                {
                    options.OnMessageRead = (gateway, channel, dateTime, cloudEvent, serviceProvider) =>
                    {
                        DataStore.Events.Add(cloudEvent);
                    };
                }
            );
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
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
