using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.Gateways;

namespace Weikio.EventFramework.Samples.CodeConfiguration
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
            services.AddRazorPages();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            services.AddEventFramework()
                .AddLocal()
                .AddHttp("web");

//                .AddLocal("localpriority", 3)
//                .AddHttp()
//                .AddHttp("priority", "/myCloudEvents", 5)
//                .AddAzureServiceBus("connectionString", "incomingqueue")
//                .AddGateway(new LocalGateway())
//                .AddGateway("red", new LocalGateway())
//                .AddRoute(cloudEvent => true, cloudEvent =>
//                {
//                  Console.WriteLine("Hello world");
//
//                  return Task.CompletedTask;
//                }) 
//                .AddRoute(x => x.Type == "hello_world", (provider, cloudEvent) =>
//                {
//                    var logger = provider.GetService<ILogger<Startup>>();
//                    logger.LogInformation("Handling message");
//
//                    return Task.CompletedTask;
//                });
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
    }
}
