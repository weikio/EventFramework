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
using Weikio.ApiFramework;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.EventSource.Api.SDK.ApiFrameworkIntegration;

namespace Weikio.EventFramework.EventSource.Http.Sample
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
                .AddEventFlow(EventFlowBuilder.From<MyTestApiEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.Id = "web";
                        options.Configuration = new MyTestApiConfiguration() { Route = "testev" };
                    })
                    .Handle(
                        (ev, provider) =>
                        {
                            var logger = provider.GetRequiredService<ILogger<Startup>>();
                            logger.LogInformation(ev.ToJson());
                        }))
                .AddEventFlow(EventFlowBuilder.From<MyTestApiEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.Id = "web2";
                        options.Configuration = new MyTestApiConfiguration() { Route = "myweb" };
                    })
                    .Handle(
                        (ev, provider) =>
                        {
                            var logger = provider.GetRequiredService<ILogger<Startup>>();
                            logger.LogInformation("MyWeb received");
                        }))
                .AddEventFlow(EventFlowBuilder.From<HttpCloudEventEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.Id = "http";
                        options.Configuration = new HttpCloudEventReceiverApiConfiguration()
                        {
                            Route = "http"
                        };
                    })
                    .Handle(
                        (ev, provider) =>
                        {
                            var logger = provider.GetRequiredService<ILogger<Startup>>();
                            logger.LogInformation("Http Event received");
                        }));
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
