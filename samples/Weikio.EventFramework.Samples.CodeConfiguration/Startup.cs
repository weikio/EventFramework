using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Configuration;
using Weikio.EventFramework.EventLinks.EventLinkFactories;
using Weikio.EventFramework.Files;

namespace Weikio.EventFramework.Samples.CodeConfiguration
{
    public class FileCreatedHandler
    {
        private readonly ILogger<FileCreatedHandler> _logger;

        public FileCreatedHandler(ILogger<FileCreatedHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(FileCreatedEvent createdEvent)
        {
            _logger.LogInformation("Received {Created}", createdEvent);
            return Task.CompletedTask;
        }
    }
    
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
            services.AddMvc();
            services.AddHttpClient();
            
            services.AddRazorPages();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            services.AddOpenApiDocument();

            services.AddEventFramework(options =>
                {
                    options.DefaultGatewayName = "local";

                    // options.TypeToEventLinksHandlerTypes.Clear();
                    // options.TypeToEventLinksHandlerTypes.Add(typeof(PublicTasksToHandlers));
                    // options.TypeToEventLinksHandlerTypes.Add(typeof(CloudEventsToTypeHandlers));
                    // options.TypeToEventLinksHandlerTypes.Add(typeof(GenericCloudEventsToTypeHandlers));
                    //
                    // // options.TypeToEventLinksFactoryTypes.Clear();
                    // // options.TypeToEventLinksFactoryTypes.Add(typeof(PublicTasksToEventLinksFactory));
                })
                .AddLocal("local")
                .AddHttp("web", "myevents/incoming", "68d6a3d2-8cb4-4236-b0f5-442ee584558f", client =>
                {
                    client.BaseAddress = new Uri("https://webhook.site");
                })
                .AddHandler<RoutingHandler>(handler =>
                {
                    handler.IncomingGatewayName = "local";
                    handler.OutgoingGatewayName = "web";
                })                
                .AddHandler<FileCreatedHandler>(nameof(FileCreatedEvent));

            services.AddHostedService<FileEventSource>();
            

            // .AddHttp("web2", "api/events")
            // .AddHandler<SaveHandler>(clo => clo.Subject == "1234");

            // .AddHandler<CustomerCreatedHandler>();

            // .AddLocal("local2")
            // .AddHttp("web", "myevents/incoming", "68d6a3d2-8cb4-4236-b0f5-442ee584558f", client =>
            // {
            //     client.BaseAddress = new Uri("https://webhook.site");
            // })
            // .AddHttp("web2", "api/events")               
            // .AddHandler(async cloudEvent =>
            // {
            //     var client = new HttpClient();
            //     client.BaseAddress = new Uri("https://webhook.site");
            //     
            //     var content = new CloudEventContent( cloudEvent,
            //         ContentMode.Structured,
            //         new JsonEventFormatter());
            //
            //     await client.PostAsync("68d6a3d2-8cb4-4236-b0f5-442ee584558f", content);
            // })
            // .AddHandler<SaveHandler>()
            // .AddHandler<SaveHandler>(handler =>
            // {
            //     handler.Path = @"c:\temp";
            // })
            // .AddHandler<RoutingHandler>(handler =>
            // {
            //     handler.IncomingGatewayName = "web";
            //     handler.OutgoingGatewayName = "local";
            // })
            // .AddHandler<RoutingHandler>(handler =>
            // {
            //     handler.IncomingGatewayName = "local";
            //     handler.OutgoingGatewayName = "web";
            //     handler.Filter = cloudEvent => cloudEvent.Subject == "123";
            //
            //     handler.OnRouting = (cloudevent, provider) =>
            //     {
            //         cloudevent.Type = "com.eventframework.modified";
            //
            //         return Task.FromResult(cloudevent);
            //     };
            // });
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

            app.UseStaticFiles();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
