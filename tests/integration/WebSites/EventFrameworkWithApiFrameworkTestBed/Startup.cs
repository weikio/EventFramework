using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Extensions.EventAggregator;

namespace EventFrameworkWithApiFrameworkTestBed
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

            services.AddApiFrameworkWithAdmin()
                .AddApi<TestApi>("/hello");
                
            services.AddEventFramework()
                .AddHandler<TestHandler>();
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
    
    public class TestEvent
    {
        public string Test { get; set; } = "For the handler";
    }

    public class TestHandler
    {
        public static bool Handled { get; set; }
        
        public Task Handle(CloudEvent cloudEvent)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    public class TestApi
    {
        public string GetHello()
        {
            return "world";
        }
    }
}
