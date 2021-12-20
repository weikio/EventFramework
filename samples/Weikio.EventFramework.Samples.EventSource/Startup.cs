using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.Samples.EventSource
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //
            // var builder = 
            //     IntegrationFlowBuilder.From<TestEventSource>(options =>
            // {
            //     options.PollingFrequency = TimeSpan.FromSeconds(1);
            //     options.Autostart = true;
            // }).Handle(ev =>
            // {
            //     var json = ev.ToJson();
            //     Console.WriteLine(json);
            // });
            //
            // services.AddEventFramework()
            //     .AddIntegrationFlow(builder);
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
