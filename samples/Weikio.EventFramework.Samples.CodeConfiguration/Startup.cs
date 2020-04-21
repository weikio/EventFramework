using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions.EventAggregator;
using Weikio.EventFramework.Files;
using Weikio.EventFramework.Router;

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

    // public class NewLinesAddedHandler
    // {
    //     private readonly ILogger<NewLinesAddedHandler> _logger;
    //
    //     public NewLinesAddedHandler(ILogger<NewLinesAddedHandler> logger)
    //     {
    //         _logger = logger;
    //     }
    //
    //     public Task Handle(NewLinesAddedEvent createdEvent)
    //     {
    //         _logger.LogInformation("Received {Created}", createdEvent);
    //         return Task.CompletedTask;
    //     }
    // }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public class CounterHandler
        {
            public Task Handle(CounterEvent counterEvent)
            {
                Console.WriteLine($"{DateTime.Now}: {counterEvent.Count}");

                return Task.CompletedTask;
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddHttpClient();

            services.AddRazorPages();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            services.AddOpenApiDocument();

            // services.Configure<CloudEventPublisherOptions>(options =>
            // {
            //     options.DefaultGatewayName = "local";
            // });
            //
            var builder = services.AddEventFramework()
                .AddLocal("local")
                .AddHttpGateway("web", "myevents/incoming", "16247d99-b297-4711-899a-e4e8f78c13d5", client =>
                {
                    client.BaseAddress = new Uri("https://webhook.site");
                })
                .AddRoute("local", "web");

            // .AddHandler<CounterHandler>();

            // .AddHandler(cl =>
            // {
            //     Console.WriteLine($"{DateTime.Now.ToString()} Received event");
            //
            //     return Task.CompletedTask;
            // });

            // builder.AddSource(typeof(HelloWorld2), TimeSpan.FromSeconds(5), null, new Action<HelloWorld2>(x => x.Folder = @"c:\short"));
            // builder.AddSource(typeof(HelloWorld2), TimeSpan.FromSeconds(10), null, new Action<HelloWorld2>(x => x.Folder = @"c:\longer"));

            // builder.AddSource<int>(currentCount =>
            // {
            //     currentCount += 1;
            //     var result = new CounterEvent(currentCount);
            //
            //     return (result, currentCount);
            // }, TimeSpan.FromSeconds(3));
            //
            // builder.AddSource<int>(async currentCount =>
            // {
            //     await Task.Delay(TimeSpan.FromSeconds(1));
            //     currentCount += 10;
            //     var result = new CounterEvent(currentCount);
            //
            //     return (result, currentCount);
            // }, TimeSpan.FromSeconds(2));

            // builder.AddSource(currentCount =>
            // {
            //     var count = 0;
            //
            //     if (currentCount != null)
            //     {
            //         count = (int) currentCount;
            //     }
            //
            //     var result = new CounterEvent(count);
            //
            //     // updateState(count + 1);
            //
            //     return Task.FromResult<(object, object)>((result, count + 1));
            // }, TimeSpan.FromSeconds(3));

            // builder.AddSource(() =>
            // {
            //     var res = new NewLinesAddedEvent(new List<string>());
            //
            //     return Task.FromResult<object>(res);
            // }, TimeSpan.FromSeconds(3));

            // builder.AddSource(() =>
            // {
            //     var result = new List<object>();
            //
            //     for (var i = 0; i < 5; i++)
            //     {
            //         result.Add(new NewLinesAddedEvent(new List<string>()));
            //     }
            //     
            //     return Task.FromResult(result);
            // }, TimeSpan.FromSeconds(10));

            // var jobSchedule = new JobSchedule(typeof(HelloWorld2), "0/30 * * * * ?") { Configure = new Action<HelloWorld2>(x => x.Folder = @"c:\temp\long") };
            // services.AddSingleton(jobSchedule);
            //
            // var implementationInstance =
            //     new JobSchedule(typeof(HelloWorld2), TimeSpan.FromSeconds(5)) { Configure = new Action<HelloWorld2>(x => x.Folder = @"c:\short") };
            //
            // services.AddSingleton(implementationInstance);

            // .AddHttp("web", "myevents/incoming", "68d6a3d2-8cb4-4236-b0f5-442ee584558f", client =>
            // {
            //     client.BaseAddress = new Uri("https://webhook.site");
            // })
            // .AddHandler<RoutingHandler>(handler =>
            // {
            //     handler.IncomingGatewayName = "local";
            //     handler.OutgoingGatewayName = "web";
            // })                
            // .AddHandler<NewLinesAddedHandler>(nameof(NewLinesAddedEvent));

            // services.AddHostedService<TextFileContentEventSource>();

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
