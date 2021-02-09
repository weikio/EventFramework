using System;
using System.Net.Http;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public abstract class EventFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        
        protected EventFrameworkTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected HttpClient Init(Action<IServiceCollection> action = null)
        {
            var result = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    logging.AddXUnit(Output); // Use the ITestOutputHelper instance
                });
                
            }).CreateClient();

            return result;
        }
    }
}
