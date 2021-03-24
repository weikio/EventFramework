using System;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventGateway;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public abstract class ChannelTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        
        protected ChannelTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
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
                    XUnitLoggerExtensions.AddXUnit((ILoggingBuilder) logging, Output); // Use the ITestOutputHelper instance
                });

            });

            return result.Services;
        }
        
        protected WebApplicationFactory<Startup> InitFactory(Action<IServiceCollection> action = null)
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

            });

            return result;
        }
    }
}
