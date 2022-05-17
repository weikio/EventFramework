using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventSource.Schedule;

namespace Weikio.EventFramework.Components.Http.Sample
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

            services.AddEventFramework()
                .AddEventFlow(EventFlowBuilder.From<ScheduleEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.PollingFrequency = TimeSpan.FromSeconds(5);
                    })
                    .Branch((ev => ev.To<ScheduleEvent>().Object.DateTimeLocal.Minute % 2 == 0, evenFlowBuilder =>
                    {
                        evenFlowBuilder.Component<HttpEndpoint>(endpoint =>
                        {
                            endpoint.Configuration.Endpoint = "https://webhook.site/587488ae-69de-4148-ac6b-007d82bc01b4";
                        });
                    }), (ev => ev.To<ScheduleEvent>().Object.DateTimeLocal.Minute % 2 != 0, oddFlowBuilder =>
                    {
                        oddFlowBuilder.Http("https://webhook.site/cf7dd9b9-e1d9-487b-99e7-49b093f234f1");
                    })));
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
